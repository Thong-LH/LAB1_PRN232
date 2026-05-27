namespace PRN232.LMS.Services.BusinessModels;

public class CollectionQueryBusinessModel
{
    public string? Search { get; set; }

    public string? Sort { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Expand { get; set; }
}
