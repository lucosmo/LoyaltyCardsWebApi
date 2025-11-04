using LoyaltyCardsWebApi.API.Common;

namespace LoyaltyCardsWebApi.API.Tests.Common;

[TestFixture]
public class ResultTest
{
    [Test]
    public void NoContentFactory_CreateNoContentResult_CorrectResultValues()
    {
        var result = Result<object?>.NoContent();

        Assert.That(result.Success, Is.True);
        Assert.That(result.SuccessType, Is.EqualTo(SuccessTypes.NoContent));
        Assert.That(result.Value, Is.Null);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorTypes.None));
        Assert.That(result.Error, Is.EqualTo(string.Empty));
        Assert.That(result.Location, Is.Null);
    }

    [Test]
    public void CreatedFactory_CreateCreatedResult_CorrectResultValues()
    {
        var result = Result<object?>.Created("created", "api/cards");

        Assert.That(result.Success, Is.True);
        Assert.That(result.SuccessType, Is.EqualTo(SuccessTypes.Created));
        Assert.That(result.Value, Is.EqualTo("created"));
        Assert.That(result.ErrorType, Is.EqualTo(ErrorTypes.None));
        Assert.That(result.Error, Is.EqualTo(string.Empty));
        Assert.That(result.Location, Is.EqualTo("api/cards"));
    }
}