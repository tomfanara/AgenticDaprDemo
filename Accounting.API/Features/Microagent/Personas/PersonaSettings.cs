namespace Accounting.API.Features.Microagent.Personas
{
    public class PersonaSettings
    {
        private string tone;
        private string style;
        private List<string> traits;

        public void SetTone(string tone)
        {
            this.tone = tone;
        }

        public void SetStyle(string style)
        {
            this.style = style;
        }

        public void SetTraits(List<string> traits)
        {
            this.traits = traits;
        }

        public string GenerateResponse(string input)
        {
            // Simplified response generation based on persona settings
            return $"[{tone} {style}]: " + input;
        }
    }
}
