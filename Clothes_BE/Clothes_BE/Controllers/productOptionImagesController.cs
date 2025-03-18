using Clothes_BE.DTO;
using Clothes_BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Any;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Clothes_BE.Controllers
{
    [Route("api/product-option-images")]
    [ApiController]
    public class productOptionImagesController : ControllerBase
    {
        // GET: api/<productOptionImagesController>
        public readonly DatabaseContext _databaseContext;
        public productOptionImagesController(DatabaseContext databaseContext)
        {
            this._databaseContext = databaseContext;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductOptionImages>>> get()
        {
            var data = await _databaseContext.product_option_images.ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> get_id(int id)
        {
            var isValid = await _databaseContext.product_option_images.FindAsync(id);
            if (isValid == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });
            return Ok(new Response { status = 200, message = "Thành công", data = isValid });
        }
        [HttpGet("filter")]
        public async Task<ActionResult> filter(int? product_id, int? option_value_id)
        {
            var product_images = await _databaseContext.product_option_images.ToListAsync();
            if (product_id != null)
            {
                var isValid = product_images.FirstOrDefault(p => p.product_id == product_id);
                if (isValid == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });
                product_images = product_images.Where(p => p.product_id == product_id).ToList();
            }

            if (option_value_id != null)
            {
                var isValid = product_images.FirstOrDefault(p => p.option_value_id == option_value_id);
                if (isValid == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });
                product_images = product_images.Where(p => p.option_value_id == option_value_id).ToList();
            }

            return Ok(new Response { status = 200, message = "Thành công", data = product_images });
        }
        
        // POST api/<productOptionImagesController>
        [HttpPost]
        public async Task<ActionResult<ProductOptionImages>> Post([FromForm] ProductImageDTO productImageDTO)
        {
               
            //get product => label(category)
            var label = await _databaseContext.products.Include(c=>c.categories).FirstOrDefaultAsync(x=>x.id == productImageDTO.product_id);
            //get option_value => label(option_value)
            var options = await _databaseContext.option_values.FirstOrDefaultAsync(x => x.id == productImageDTO.option_value_id);
            //get same file name //label.categories.label + "-" + productImageDTO.product_id;
            string get_file_name = $"{label.categories.label}-{productImageDTO.product_id}";
            var productFolder = Path.Combine(Directory.GetCurrentDirectory(), "images", $"{get_file_name}");    
            //var productFolder = Path.Combine(Directory.GetCurrentDirectory(), "images");
            //check extension file
            List<string> isImage = new List<string> { ".jpg", ".png", ".gif" };
            //
            //
            var check_data = _databaseContext.product_options
               .Where(x => x.product_id == productImageDTO.product_id)
               .Select(g => g.option_id).ToList();
            var option_value = _databaseContext.option_values.ToList();
            //mapper đã ràng buộc khóa ngoại nên khi thêm không có tự báo lỗi     
            using (var transactions = _databaseContext.Database.BeginTransaction()) { 
                try
                {
                    //check option 
                    var option_item = option_value.FirstOrDefault(p => p.id == productImageDTO.option_value_id);
                    if (!check_data.Contains(option_item.option_id)) return BadRequest(new Response { status = 400, message = "Option không có trong ràng buộc của sản phẩm" });
                    //Tạo thư mục: [lable-product_id]                
                    if (!Directory.Exists(productFolder))
                    {
                        Directory.CreateDirectory(productFolder);
                    }
                    //first image => color of product follow option_value
                    foreach (var file in productImageDTO.files)
                    {
                        //set file name
                        var file_name = $"MAU-{get_file_name}-{options.label}";
                        if (productImageDTO.files.First() == file) file_name = $"CHITIET-{get_file_name}-{options.label}";
                        //check extension file
                        if (!isImage.Contains(Path.GetExtension(file.FileName))) return BadRequest(new Response { status = 400, message = "file không hợp lệ" });                       
                        //set file path
                        var file_path = Path.Combine(Directory.GetCurrentDirectory(),"images", productFolder, file_name + Path.GetExtension(file.FileName));
                        int count = 1;
                        while(System.IO.File.Exists(file_path))
                        {
                            file_name = string.Format("{0}({1})", $"MAU-{get_file_name}-{options.label}", count);
                            file_path = Path.Combine(Directory.GetCurrentDirectory(), "images", productFolder, file_name + Path.GetExtension(file.FileName));
                            count++;
                        }
                        //save file
                        using (var stream = new FileStream(file_path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        //create object
                        var productImages = new ProductOptionImages
                        {
                            product_id = productImageDTO.product_id,
                            option_value_id = productImageDTO.option_value_id,
                            //src = $"/{get_file_name}/{file_name}"
                            src = $"/{get_file_name}/{file_name + Path.GetExtension(file.FileName)}"
                        };
                        //add
                        _databaseContext.product_option_images.AddRange(productImages);
                    }
                    //save
                    await _databaseContext.SaveChangesAsync();
                    transactions.Commit();
                }
                catch
                {
                    transactions.Rollback();
                    return BadRequest(new Response { status = 400, message = "Thất bại" });
                }
            }
            return Ok(new Response { status = 200, message = "Thành công"});

        }

        //Chỉ đổi hình ảnh
        //=> lấy hình ảnh và sắp xếp tăng/giàm theo tên => MAU => M ; CHITIET => 
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm]FileModel productImageDTO)
        {
           
            try
            {
                //
                var label = await _databaseContext.products.Include(c => c.categories).FirstOrDefaultAsync(x => x.id == productImageDTO.product_id);
                //get option_value => label(option_value)
                var options = await _databaseContext.option_values.FirstOrDefaultAsync(x => x.id == productImageDTO.option_value_id);
                //get same file name
                string get_file_name = $"{label.categories.label}-{productImageDTO.product_id}";
                //folder image
                var productFolder = Path.Combine(Directory.GetCurrentDirectory(), "images", $"{get_file_name}");              
                //check extension file
                List<string> isImage = new List<string> { ".jpg", ".png", ".gif" };
                if (!isImage.Contains(Path.GetExtension(productImageDTO.files.FileName))) return BadRequest(new Response { status = 400, message = "file không hợp lệ" });               
                if (id != productImageDTO.id) return BadRequest(new Response { status = 400, message = "Không khớp id" });
                var isProductImage = await _databaseContext.product_option_images.FindAsync(id);
                if (isProductImage == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });

                //delete old image
                var file_path_old = Path.GetFileName(isProductImage.src);
                var folderrparent = Path.GetDirectoryName(isProductImage.src);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "images", folderrparent.Replace("\\",""), file_path_old);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                //add new file
                if (!Directory.Exists(productFolder))
                {
                    Directory.CreateDirectory(productFolder);
                }
                //set file name
                var file_name = $"MAU-{get_file_name}-{options.label}";
                file_name = $"CHITIET-{get_file_name}-{options.label}";              
                //set file path
                var file_path = Path.Combine(Directory.GetCurrentDirectory(), "images", productFolder, file_name + Path.GetExtension(productImageDTO.files.FileName));
                int count = 1;
                while (System.IO.File.Exists(file_path))
                {
                    file_name = string.Format("{0}({1})", $"MAU-{get_file_name}-{options.label}", count);
                    file_path = Path.Combine(Directory.GetCurrentDirectory(), "images", productFolder, file_name + Path.GetExtension(productImageDTO.files.FileName));
                    count++;
                }
                //save file
                using (var stream = new FileStream(file_path, FileMode.Create))
                {
                    await productImageDTO.files.CopyToAsync(stream);
                }
                //create object
                //var productImages = new ProductOptionImages
                //{
                //    product_id = productImageDTO.product_id,
                //    option_value_id = productImageDTO.option_value_id,
                //    //src = $"/{get_file_name}/{file_name}"
                //    src = $"/{get_file_name}/{file_name + Path.GetExtension(productImageDTO.files.FileName)}"
                //};
                //update object
                isProductImage.product_id = productImageDTO.product_id;
                isProductImage.option_value_id = productImageDTO.option_value_id;
                isProductImage.src = $"/{get_file_name}/{file_name + Path.GetExtension(productImageDTO.files.FileName)}";

                //add
                //_databaseContext.product_option_images.Add(productImages);
                await _databaseContext.SaveChangesAsync();
                return Ok(new Response { status = 200, message = "Thành công", data=path});
            }
            catch
            {
                return BadRequest(new Response { status = 400, message = "Thất bại" });
            }
           
        }

        // DELETE api/<productOptionImagesController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var isProductImage = await _databaseContext.product_option_images.FindAsync(id);
                if(isProductImage == null) return BadRequest(new Response { status = 400, message = "Không tìm thấy id" });

                var file_path_old = Path.GetFileName(isProductImage.src);
                var folderrparent = Path.GetDirectoryName(isProductImage.src);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "images", folderrparent.Replace("\\", ""), file_path_old);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                _databaseContext.Remove(isProductImage);
                await _databaseContext.SaveChangesAsync();
            }
            catch {
                return BadRequest(new Response { status = 400, message = "Thất bại" });
            }
            return Ok(new Response { status = 200, message = "Thành công", });
        }        
    }
}
