import { Service } from '@angular/core';
import { Observable } from 'rxjs';

@Service()
export class ChatService {
  ask(message: string): Observable<string> {
    return new Observable<string>((subscriber) => {
      const abort = new AbortController();

      (async () => {
        try {
          const response = await fetch('/api/chat', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              Accept: 'text/event-stream',
            },
            body: JSON.stringify({ message }),
            signal: abort.signal,
          });

          if (!response.ok || !response.body) {
            subscriber.error(
              new Error(`Chat request failed: ${response.status} ${response.statusText}`),
            );
            return;
          }

          const reader = response.body.pipeThrough(new TextDecoderStream()).getReader();

          let buffer = '';

          while (true) {
            const { value, done } = await reader.read();
            if (done) break;

            buffer += value;

            const lines = buffer.split('\n');
            buffer = lines.pop() ?? '';

            for (const piece of this.parseLines(lines)) {
              subscriber.next(piece);
            }
          }

          // Anything left in the buffer after the stream ends
          for (const piece of this.parseLines([buffer])) {
            subscriber.next(piece);
          }

          subscriber.complete();
        } catch (err) {
          if (err instanceof DOMException && err.name === 'AbortError') {
            subscriber.complete();
          } else {
            subscriber.error(err);
          }
        }
      })();

      // Called when the subscriber unsubscribes: cancels the HTTP request,
      // which cancels the request token on the API and stops the model.
      return () => abort.abort();
    });
  }

  private *parseLines(lines: string[]): Generator<string> {
    for (const raw of lines) {
      const line = raw.trimEnd(); // removes the '\r' if the server sends '\r\n'
      if (!line.startsWith('data:')) continue;

      const data = line.slice('data:'.length).trimStart();
      if (!data) continue;

      try {
        const chunk = JSON.parse(data) as { text?: string };
        if (chunk.text) yield chunk.text;
      } catch {
        // Not valid JSON: ignore this line
      }
    }
  }
}
