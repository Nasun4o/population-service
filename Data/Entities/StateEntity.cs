namespace PopulationService.Data.Entities;

public class StateEntity
{
    public int StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public int CountryId { get; set; }

    public CountryEntity Country { get; set; } = null!;
    public ICollection<CityEntity> Cities { get; set; } = [];
}
