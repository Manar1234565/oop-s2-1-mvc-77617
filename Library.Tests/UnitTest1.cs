namespace Library.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ThisTestshouldPass()
        {
            string name = "Manar";
            Assert.Equal("Manar", name);
        }


    }
}