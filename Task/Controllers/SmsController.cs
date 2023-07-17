using Microsoft.AspNetCore.Mvc;
using Task.Models;
using System.Text.RegularExpressions;


namespace Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly SmsMisrProperties _smsConfig;

        public SmsController()
        {
            _smsConfig = new SmsMisrProperties(
                "ed810713d5566299555e961265f91315e5ec169924727e38eeac06b2fb736b13",
                "b611afb996655a94c8e942a823f1421de42bf8335d24ba1f84c437b2ab11ca27",
                "c96c511a226d104e40251607398596bc231ffcce63207b67861ef1e94672642e");
        }
        [HttpPost]
        public async Task<ActionResult> SendSmsMessageSingleOrMultiple(List<string> mobileNumbers, string message, string language, decimal maxCostMustSmsTake, DateTime timeToSendThisSms)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    return BadRequest("Message cannot be empty");
                }
                if (maxCostMustSmsTake <= 0)
                {
                    return BadRequest("Invalid MaxCost");
                }

                string dateTime = "";
                if (timeToSendThisSms != default(DateTime))
                {
                    dateTime = timeToSendThisSms.ToString("yyyy-MM-ddTHH:mm:ssZ"); // 2023-07-05T10:00:00.00Z
                }
                else
                {
                    timeToSendThisSms = DateTime.Now;
                }

                int lang = language.ToLower() == "english" || language.ToLower() == "en" ? 1 : 2;
                decimal maxCharacterToComputeOneCost = lang == 2 ? 70m : 160m;
                decimal maxCharacterToThisMessage = maxCostMustSmsTake * maxCharacterToComputeOneCost;
                decimal characterThisMessageTake = (mobileNumbers.Count * message.Length);
                decimal costThisMessageTake = characterThisMessageTake / maxCharacterToComputeOneCost;
                decimal roundNumber = Math.Round(costThisMessageTake, 2);
                if (maxCharacterToThisMessage <= characterThisMessageTake)
                {
                    return BadRequest($"The cost is {costThisMessageTake} More than MaxCost that You Enter");
                }

                Regex regex = new Regex(@"^(?:\+2)?01[0125]\d{8}$");
                foreach (string number in mobileNumbers)
                {
                    if (!regex.IsMatch(number))
                    {
                        return BadRequest("Invalid phone number");
                    }
                }

                var encodedMessage = Uri.EscapeDataString(message);
                var url = $"https://smsmisr.com/api/SMS/?environment=2&username={_smsConfig.Username}&password={_smsConfig.Password}&sender={_smsConfig.SenderID}&mobile={string.Join(",", mobileNumbers)}&language={lang}&message={encodedMessage}&DelayUntil={dateTime}";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.PostAsync(url, null);
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Handle the response from the SMS provider's API
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        // stored successfull sms in database
                        var mobileNumbersString = string.Join(",", mobileNumbers);
                        var sms = new SuccessfullyMessage(mobileNumbersString, message, language, costThisMessageTake, timeToSendThisSms);
                        await sms.InsertIntoDatabaseAsync();
                        return Ok($"SMS send and stored Successfully and Take {costThisMessageTake}");
                    }
                    else
                    {
                        return BadRequest("SMS failed to send");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error sending SMS: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        [HttpPost("OTP")]
        public async Task<ActionResult> SendOTPAsync(string mobileNumber)
        {
            try
            {
                Regex regex = new Regex(@"^(?:\+2)?01[0125]\d{8}$");
                if (regex.IsMatch(mobileNumber))
                {
                    // Generate a random 6-digit OTP
                    Random random = new Random();
                    int otpValue = random.Next(100000, 999999);

                    // Set the SMS template token and URL using the generated OTP
                    string templateToken = "0f9217c9d760c1c0ed47b8afb5425708da7d98729016a8accfc14f9cc8d1ba83";
                    string url = $"https://smsmisr.com/api/OTP/?environment=2&username={_smsConfig.Username}&password={_smsConfig.Password}&sender={_smsConfig.SenderID}&mobile={mobileNumber}&template={templateToken}&otp={otpValue}";

                    HttpClient client = new HttpClient();
                    HttpResponseMessage response = await client.PostAsync(url, null);
                    response.EnsureSuccessStatusCode();
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent != null)
                    {
                        return Ok($"OTP sent successfully to {mobileNumber}");
                    }
                    else
                    {
                        return BadRequest($"Failed to send OTP to {mobileNumber}");
                    }
                }
                return Ok();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error sending OTP message: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}

