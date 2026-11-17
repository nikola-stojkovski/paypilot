namespace PayPilot.Infrastructure.Ai.Prompts;

// Prompts are Markdown files in this folder, embedded into the assembly at build time.
public static class PromptLibrary
{
    public static string Get(string name)
    {
        var resourceName = $"Prompts/{name}.md";

        using var stream = typeof(PromptLibrary).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Prompt '{resourceName}' not found. Check that the file exists and is an EmbeddedResource.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Trim();
    }
}