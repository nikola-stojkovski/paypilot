import { Component, DestroyRef, inject, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { ChatService } from '../chat.service';

@Component({
  imports: [],
  selector: 'app-chat',
  styleUrl: './chat.scss',
  templateUrl: './chat.html',
})
export class Chat {
  private readonly chatService = inject(ChatService);

  protected readonly question = signal('');
  protected readonly answer = signal('');
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  private subscription?: Subscription;

  constructor() {
    // Cancel a running request if the user leaves the page
    inject(DestroyRef).onDestroy(() => this.subscription?.unsubscribe());
  }

    protected send(): void {
    const message = this.question().trim();
    if (!message || this.loading()) return;

    this.answer.set('');
    this.error.set(null);
    this.loading.set(true);

    this.subscription = this.chatService.ask(message).subscribe({
      next: piece => this.answer.update(current => current + piece),
      error: err => {
        this.error.set(err instanceof Error ? err.message : String(err));
        this.loading.set(false);
      },
      complete: () => this.loading.set(false),
    });
  }

  protected stop(): void {
    // Unsubscribing aborts the fetch, which cancels the request on the API and stops the model
    this.subscription?.unsubscribe();
    this.loading.set(false);
  }

}
