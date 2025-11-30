using System.Text.Json.Serialization;

namespace CreatureChat.CatDialogue
{
    public class RootDialogue
    {
        [JsonPropertyName("sympathy and nervous")]
        public SympathyAndNervous SympathyAndNervous { get; set; } = new();

        [JsonPropertyName("bravery and aggression")]
        public BraveryAndAggression BraveryAndAggression { get; set; } = new();

        [JsonPropertyName("energy and dominance")]
        public EnergyAndDominance EnergyAndDominance { get; set; } = new();
    }
}
