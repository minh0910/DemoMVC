using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers
{
    // Bài 11: lớp cha cho các controller CRUD dùng Modal + Ajax.
    // Quy ước action:
    //  - Index        : trang khung (ô tìm kiếm, nút Thêm, vùng chứa bảng)
    //  - List         : partial _Table, JS gọi lại sau mỗi lần thêm / sửa / xoá
    //  - Form(id)     : partial _Form hiển thị trong modal (id = null là thêm mới)
    //  - Details(id)  : partial _Details hiển thị trong modal
    //  - Save         : thành công trả JSON, lỗi validation trả lại _Form (HTTP 400) để hiện lỗi trong modal
    //  - Delete       : trả JSON
    // Bài 13: các trang quản lý dữ liệu phải đăng nhập mới vào được
    [Authorize]
    public abstract class AjaxCrudController : Controller
    {
        protected IActionResult JsonOk(string message) => Json(new { success = true, message });

        protected IActionResult JsonFail(string message) => Json(new { success = false, message });

        protected IActionResult FormWithErrors(object model)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return PartialView("_Form", model);
        }
    }
}
