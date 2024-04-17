using AntSK.plugins.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AntSK.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JesseController(FunctionTest functionTest) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<string>> TestLInk() {

            var response = functionTest.GetWorkingHours("ZHOUZJ");

            return Ok(response);
        }
    }
}
