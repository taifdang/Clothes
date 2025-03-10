using Clothes_BE.DTO;
using Clothes_BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Clothes_BE.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class productsController : ControllerBase
    {
        // GET: api/<productsController>
        public readonly DatabaseContext _databaseContext;
        public productsController(DatabaseContext databaseContext)
        {
            this._databaseContext = databaseContext;
        }
        [HttpGet]
        public ActionResult Get()
        {
            //var data = _databaseContext.products.Include(x=>x.product_attributes).Include(x => x.product_variants).ToList();
           
            //var product = data.Select(x => new
            //{
            //   x.id,
            //   x.title,
            //   x.price,
            //   x.old_price,
            //   attributes = x.product_attributes.ToList(),
            //   variants = x.product_variants.Select(variant =>
            //   {
            //       int lastSpaceIndex = variant.title.LastIndexOf(' ');
            //       return new ProductVariantDTO
            //       {
            //           id = variant.id,
            //           title = variant.title,
            //           price = variant.price,
            //           old_price = variant.old_price,
            //           quantity = variant.quantity,
            //           option1 = variant.title.Substring(0,lastSpaceIndex).Trim(),
            //           option2 = variant.title.Substring(lastSpaceIndex+1).Trim(),
            //           product_id = variant.product_id,
            //           product_title = variant.products.title,
            //           percent = variant.percent,

            //       };
            //   }).ToList(),
            //   options = x.product_variants
            //        .Select(p =>
            //        {
            //            int lastSpaceIndex = p.title.LastIndexOf(' ');
            //            return new { 
            //                title = "color",
            //                value = p.title.Substring(0, lastSpaceIndex).Trim(),
            //            };

            //        }) // Lấy phần màu sắc từ title
            //        .Distinct()
            //        .GroupBy(x=>x.title == "color")
            //        .Select(g => new
            //         {                       
            //             values = g.Select(x => x.value).Distinct().ToList()
            //        })
            //        .ToList()
            //}).ToList();
            
        
            return Ok();
        }

        // GET api/<productsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<productsController>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<productsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<productsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
