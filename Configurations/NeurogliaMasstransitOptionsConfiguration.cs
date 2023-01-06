namespace MasstransitTest.WebApiTest.Configurations;

public class NeurogliaMasstransitOptionsConfiguration
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public License? License { get; set; }
}

public class License
{
    public string Name { get; set; }
    public Uri LicenseUrl { get; set; }

    public License(string name, Uri licenseUrl)
    {
        Name = name;
        LicenseUrl = licenseUrl;
    }
}