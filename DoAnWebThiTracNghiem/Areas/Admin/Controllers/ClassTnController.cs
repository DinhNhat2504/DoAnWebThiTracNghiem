﻿using DoAnWebThiTracNghiem.Areas.Admin.Models;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Drawing.Printing;

namespace DoAnWebThiTracNghiem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClassTnController : Controller
    {
        private readonly IClassTnRepository _Clcontext;
        public ClassTnController(IClassTnRepository clcontext)
        {
            _Clcontext = clcontext;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string search = "")
        {
            var UserId = HttpContext.Session.GetString("UserId");
            var RoleId = HttpContext.Session.GetString("RoleId");
            int userId = int.Parse(UserId);
            int roleId = int.Parse(RoleId);
            var total = await _Clcontext.CountAsync(roleId, userId, search);
            var items = await _Clcontext.GetPagedAsync(roleId, userId, page, pageSize, search);

            var model = new PagedResult<ClassTn>
            {
                Items = items,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                TotalItems = total,
                Search = search
            };
            var subjects = await _Clcontext.GetAllSubjectsAsync(1, userId);
            ViewData["Subjects"] = subjects.Select(s => new
            {
                Subject_Id = s.Subject_Id,
                Subject_Display = $"{s.Subject_Name} - {s.CreatorName}"
            }).ToList();

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetSubjectsForClass(int classId)
        {
            var classTn = await _Clcontext.GetByIdAsync(classId);
            if (classTn == null)
                return Json(new List<Subject>());
            var subjects = await _Clcontext.GetSubjectsByCreatorAsync(classTn.CreatorUser_Id);
            return Json(subjects ?? new List<Subject>());
        }


        public async Task<IActionResult> Details(int id)
        {
            var classInfo = await _Clcontext.GetByIdAsync(id);
            if (classInfo == null) return NotFound();

            // Fix: Map Exams to Exam_Class objects
            var exams = (await _Clcontext.GetExamsOfClassAsync(id))
                .Select(exam => new Exam_Class
                {
                    Exam_ID = exam.Exam_ID,
                    ClassTNClass_Id = id,
                    Exam = exam,
                    AssignedAt = DateTime.Now // Adjust as needed
                }).ToList();

            var students = (await _Clcontext.GetStudentsInClassAsync(id))
                .Select(user => new Student_Class
                {
                    User_ID = user.User_Id,
                    Class_ID = id,
                    Timestamp = DateTime.Now, // Adjust as needed
                    User = user
                }).ToList();

            var notifications = await _Clcontext.GetNotificationsOfClassAsync(id);

            var model = new ClassDetailViewModel
            {
                Class = classInfo,
                Students = students,
                Exams = exams,
                Notifications = notifications
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClassTn classTn)
        {
            var userID = HttpContext.Session.GetString("UserId");
            classTn.CreatorUser_Id = int.Parse(userID);

            if (ModelState.IsValid)
            {

                classTn.CreatedAt = DateTime.Now;
                classTn.UpdatedAt = DateTime.Now;
                classTn.InviteCode = await GenerateUniqueInviteCode();
                await _Clcontext.AddAsync(classTn);
                
                return Json(new { success = true, message = "Thêm lớp học thành công !" });
            }
            // Trả về lỗi ModelState dạng JSON
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { success = false, errors });

        }
        // API lấy thông tin lớp học
        [HttpGet]
        public async Task<IActionResult> GetClass(int id)
        {
            var classTn = await _Clcontext.GetByIdAsync(id);
            if (classTn == null) return NotFound();
            return Json(classTn);
        }

        // API sửa lớp học
        [HttpPost]
        public async Task<IActionResult> EditInline(ClassTn classTn)
        {
            if (classTn == null) return BadRequest();
            var existing = await _Clcontext.GetByIdAsync(classTn.Class_Id);
            if (existing == null) return BadRequest();
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(new { success = false, errors });
            }
            existing.ClassName = classTn.ClassName;
            existing.SubjectId = classTn.SubjectId;
            existing.UpdatedAt = DateTime.Now;
            await _Clcontext.UpdateAsync(existing);
            return Ok();
        }

        // API xóa lớp học
        [HttpPost]
        public async Task<IActionResult> DeleteInline(int id)
        {
            var classTn = await _Clcontext.GetByIdAsync(id);
            if (classTn == null) return NotFound();
            await _Clcontext.DeleteAsync(id);
            return Ok();
        }
        private async Task<string> GenerateUniqueInviteCode()
        {
            string inviteCode;
            bool exists;

            do
            {
                // Tạo mã InviteCode gồm 7 ký tự chữ hoa và số
                inviteCode = GenerateRandomCode(7);

                // Kiểm tra xem mã này đã tồn tại trong cơ sở dữ liệu chưa
                exists = await _Clcontext.Check(inviteCode);
            } while (exists);

            return inviteCode;
        }
        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
