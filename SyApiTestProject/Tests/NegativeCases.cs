using System.Net.Http.Json;
using Xunit;

namespace SyApiTestProject.Tests
{
    public class NegativeCases
    {
        private readonly HttpClient _client;

        #region API URLs

        private const string PostSignup = "https://randomlyapi.symphony.is/api/auth/signup/";

        #endregion
        #region Constructor
        public NegativeCases()
        {
            _client = new HttpClient();
        }
        #endregion
        [Theory]
        [InlineData("invalidemail.com", null, null, null, null, null)]
        [InlineData(null, "test", null, null, null, null)]
        [InlineData(null, null, "", null, null, null)]
        [InlineData(null, null, null, "", null, null)]
        [InlineData(null, null, null, null, "test", null)]
        [InlineData(null, null, null, null, null, "01/01/2030")]
        public async Task Signup_Failure(string email, string password, string firstName, string lastName, string username, string dateOfBirth)

        {
            // Arrange
            var userData = new
            {
                email = email?? "regular@mail.com",
                password = password ?? "Test123!",
                firstName = firstName ?? "Test",
                lastName = lastName ?? "User",
                username = username ?? "testuser",
                dateOfBirth =dateOfBirth ?? "01/01/2000"
            };

            // Act
            var response = await _client.PostAsJsonAsync(PostSignup, userData);

            // Assert
            Assert.False(response.IsSuccessStatusCode);

        }
    }
}