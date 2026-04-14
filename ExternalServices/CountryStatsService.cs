using PopulationService.Interfaces;
using PopulationService.Models;

namespace PopulationService.ExternalServices;

public class CountryStatsService : ICountryStatsService
{
    public Task<List<CountryPopulation>> GetCountryPopulationsAsync()
    {
        var data = new List<CountryPopulation>
        {
            new("India", 1182105000),
            new("United Kingdom", 62026962),
            new("Chile", 17094270),
            new("Mali", 15370000),
            new("Greece", 11305118),
            new("Armenia", 3249482),
            new("Slovenia", 2046976),
            new("Saint Vincent and the Grenadines", 109284),
            new("Bhutan", 695822),
            new("Aruba (Netherlands)", 101484),
            new("Maldives", 319738),
            new("Mayotte (France)", 202000),
            new("Vietnam", 86932500),
            new("Germany", 81802257),
            new("Botswana", 2029307),
            new("Togo", 6191155),
            new("Luxembourg", 502066),
            new("U.S. Virgin Islands (US)", 106267),
            new("Belarus", 9480178),
            new("Myanmar", 59780000),
            new("Mauritania", 3217383),
            new("Malaysia", 28334135),
            new("Dominican Republic", 9884371),
            new("New Caledonia (France)", 248000),
            new("Slovakia", 5424925),
            new("Kyrgyzstan", 5418300),
            new("Lithuania", 3329039),
            new("United States of America", 309349689)
        };

        return Task.FromResult(data);
    }
}
