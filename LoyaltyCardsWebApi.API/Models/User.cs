public class User
{
    public long Id { get; set;}
    public required string UserName { get; set;}
    public string? FirstName { get; set;}
    public string? LastName { get; set;}
    public DateTime AccountCreatedDate { get; set;}
    public List<Card>? Cards; 
}