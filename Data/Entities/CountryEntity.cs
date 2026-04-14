namespace PopulationService.Data.Entities;

public class CountryEntity
{
    public int CountryId { get; set; }
    public string? CountryName { get; set; }

    public ICollection<StateEntity> States { get; set; } = [];
}
