namespace PopulationService.Data.Entities;

public class CityEntity
{
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public int StateId { get; set; }
    public int? Population { get; set; }

    public StateEntity State { get; set; } = null!;
}
