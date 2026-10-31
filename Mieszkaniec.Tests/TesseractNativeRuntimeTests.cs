using Tesseract;

namespace Mieszkaniec.Tests;

public class TesseractNativeRuntimeTests
{
    [Fact]
    public void EngineStartsWithBundledNativeLibraries()
    {
        var tessdata = Path.Combine(AppContext.BaseDirectory, "tessdata");

        using var engine = new TesseractEngine(tessdata, "pol", EngineMode.LstmOnly);

        Assert.NotNull(engine);
    }
}
