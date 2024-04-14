using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace SyApiTestProject.Tests
{

    [Collection("Smoke_API")]
    public class SmokeTests
    {
        private readonly HttpClient _client;

        #region API URLs
        private const string PostSignup = "https://randomlyapi.symphony.is/api/auth/signup/";
        private const string PostLogin = "https://randomlyapi.symphony.is/api/auth/login/";
        private const string PostCreatePost = "https://randomlyapi.symphony.is/api/posts/";
        private const string PostComments = "https://randomlyapi.symphony.is/api/post-comments/";
        private const string GetComments = "https://randomlyapi.symphony.is/api/posts/{id}/comments/";
        #endregion

        #region Fields
        private static string _username;
        private string _password = "Test123!";
        private static string _token;
        private static string _postId;
        #endregion

        #region Constructor
        public SmokeTests()
        {
            _client = new HttpClient();
        }
        #endregion

        #region Tests

        [Fact]
        public async Task Signup_Success()
        {
            // Arrange
            _username = "testuser" + GenerateRandomString();
            var randomEmail = GenerateRandomString().ToLower() + "@mail.com";
            var randomName = GenerateRandomString();
            var randomLastname = GenerateRandomString();
            var dateOfBirthNew = GenerateRandomDateOfBirth().ToString("dd/MM/yyyy");

            var userData = new
            {
                email = randomEmail,
                password = _password,
                firstName = randomName,
                lastName = randomLastname,
                username = _username,
                dateOfBirth = dateOfBirthNew
            };
            // Act
            var response = await _client.PostAsJsonAsync(PostSignup, userData);
            _username = userData.username;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

        }

        [Fact]
        public async Task LoginUser_Success()
        {
            // Arrange
            await Signup_Success();
            var loginData = new
            {
                username = _username,
                password = _password
            };
            var jsonBody = JsonConvert.SerializeObject(loginData);

            // Act
            var response = await _client.PostAsync(PostLogin, new StringContent(jsonBody, Encoding.UTF8, "application/json"));

            //Extracting "token"
            var responseBody = await response.Content.ReadAsStringAsync();
            var token = JObject.Parse(responseBody)["token"].ToString();
            _token = token;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

        }

        [Fact]
        public async Task AddPost_Success()
        {
            //Arrange
            await LoginUser_Success();
            var postData = new
            {
                text = "New text" + GenerateRandomString(10)
            };
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", _token);
            //Act
            var response = await _client.PostAsJsonAsync(PostCreatePost, postData);

            //Extractin "id" from response
            var responseContent = await response.Content.ReadAsStringAsync();
            var postId = JObject.Parse(responseContent)["id"].ToString();
            _postId = postId;

            //Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task AddComment_Success()
        {
            //Arrange
            await LoginUser_Success();
            await AddPost_Success();
            var commentData = new
            {
                text = "New comment",
                post = _postId
            };

            //Act
            var response = await _client.PostAsJsonAsync(PostComments, commentData);

            //Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ReadComment_Success()
        {
            //Arrange
            await AddPost_Success();
            var apiUrlWithId = GetComments.Replace("{id}", _postId);
            //Act
            var response = await _client.GetAsync(apiUrlWithId);
            //Assert
            Assert.True(response.IsSuccessStatusCode);
        }
        #endregion

        #region Helpers
        private string GenerateRandomString(int length = 5)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }


        private DateTime GenerateRandomDateOfBirth()
        {
            var random = new Random();
            var years = random.Next(18, 100);

            var birthYear = DateTime.UtcNow.Year - years;
            var month = random.Next(1, 13); 
            var day = random.Next(1, DateTime.DaysInMonth(birthYear, month) + 1);

            var dateOfBirth = new DateTime(birthYear, month, day);

            return dateOfBirth;
        }
        #endregion
    }
}