using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.Services;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using System.Globalization;


namespace DoAnWebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class QuestionController : Controller
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly AppDBContext _context;
        private readonly ChatbotService _chatbotService;
        public QuestionController(IQuestionRepository questionRepository, AppDBContext context, ChatbotService chatbotService)
        {
            _questionRepository = questionRepository;
            _context = context;
            _chatbotService = chatbotService;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = "", int? subjectId = null, int? levelId = null, int? questionTypeId = null)
        {
            var UserId = HttpContext.Session.GetString("UserId");
            var RoleId = HttpContext.Session.GetString("RoleId");
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(RoleId))
            {
                return RedirectToAction("Index", "Home");
            }
            int userId = int.Parse(UserId);
            int roleId = int.Parse(RoleId);

            // Sửa lại repository để nhận thêm các tham số lọc
            var total = await _questionRepository.CountAsync(roleId, userId, search, subjectId, levelId, questionTypeId);
            var items = await _questionRepository.GetPagedAsync(roleId, userId, page, pageSize, search, subjectId, levelId, questionTypeId);

            var model = new PagedResult<Question>
            {
                Items = items,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                TotalItems = total,
                Search = search,
                SubjectId= subjectId,
                LevelId = levelId,
                QuestionTypeId = questionTypeId

            };

            ViewData["Subjects"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == userId), "Subject_Id", "Subject_Name", subjectId);
            ViewData["QuestionTypes"] = new SelectList(_context.QuestionType, "Id", "Name", questionTypeId);
            ViewData["Levels"] = new SelectList(_context.Levels, "Id", "LevelName", levelId);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            TempData["ImportError"] = "Vui lòng chọn file Excel.";
            return RedirectToAction(nameof(Index));
        }

        var userId = HttpContext.Session.GetString("UserId");
        int teacherId = int.Parse(userId);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Lấy danh sách ánh xạ tên sang ID
            var subjects = await _context.Subjects.Where(s => s.CreatorUser_Id == teacherId).ToListAsync();
        var levels = await _context.Levels.ToListAsync();
        var types = await _context.QuestionType.ToListAsync();

        var questions = new List<Question>();

        using (var stream = new MemoryStream())
        {
            await excelFile.CopyToAsync(stream);
                
                using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Dòng 1 là header
                {
                    try
                    {
                        var content = worksheet.Cells[row, 1].Text?.Trim();
                        var option1 = worksheet.Cells[row, 2].Text?.Trim();
                        var option2 = worksheet.Cells[row, 3].Text?.Trim();
                        var option3 = worksheet.Cells[row, 4].Text?.Trim();
                        var option4 = worksheet.Cells[row, 5].Text?.Trim();
                        var correct = worksheet.Cells[row, 6].Text?.Trim();
                        var subjectValue = worksheet.Cells[row, 7].Text?.Trim();
                        var levelValue = worksheet.Cells[row, 8].Text?.Trim();
                        var typeValue = worksheet.Cells[row, 9].Text?.Trim();

                        // Ánh xạ tên sang ID (nếu là số thì dùng luôn, nếu là tên thì tìm ID)
                        int subjectId = int.TryParse(subjectValue, out var sid)
                            ? sid
                            : subjects.FirstOrDefault(s => s.Subject_Name.Equals(subjectValue, StringComparison.OrdinalIgnoreCase))?.Subject_Id ?? 0;

                        int levelId = int.TryParse(levelValue, out var lid)
                            ? lid
                            : levels.FirstOrDefault(l => l.LevelName.Equals(levelValue, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;

                        int typeId = int.TryParse(typeValue, out var tid)
                            ? tid
                            : types.FirstOrDefault(t => t.Name.Equals(typeValue, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;

                        if (string.IsNullOrEmpty(content) || subjectId <= 0 || levelId <= 0 || typeId <= 0)
                            continue;

                        var q = new Question
                        {
                            Question_Content = content,
                            Subject_ID = subjectId,
                            Level_ID = levelId,
                            QuestionTypeId = typeId,
                            CreatorUser_Id = teacherId,
                            CreatedAt = DateTime.Now
                        };

                        if (typeId == 1)
                        {
                            q.Options = new List<string> { option1, option2, option3, option4 };
                            q.Correct_Option = correct;
                        }
                        else if (typeId == 2)
                        {
                            q.Options = new List<string> { "Đúng", "Sai" };
                            q.Correct_Option = correct;
                        }
                        else if (typeId == 3)
                        {
                            q.Correct_Option = correct;
                        }

                        questions.Add(q);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        if (!questions.Any())
        {
            TempData["ImportError"] = "Không có câu hỏi hợp lệ để import.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var q in questions)
        {
            await _questionRepository.AddAsync(q);
        }
        await _context.SaveChangesAsync();

        TempData["ImportSuccess"] = $"Đã import thành công {questions.Count} câu hỏi.";
        return RedirectToAction(nameof(Index));
    }

        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
            ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName");
            ViewData["QuestionTypes"] = new SelectList(_context.QuestionType, "Id", "Name");

            return View( new Question());
        }

        [HttpPost]
        public async Task<IActionResult> CreateMultiple(List<Question> questions)
        {
            if (questions == null || !questions.Any())
            {
                ModelState.AddModelError("", "Không có câu hỏi nào để thêm.");
                Console.WriteLine("Không có câu hỏi nào để thêm.");
                ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == int.Parse(HttpContext.Session.GetString("UserId"))), "Subject_Id", "Subject_Name");
                ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName");
                ViewData["QuestionTypes"] = new SelectList(_context.QuestionType, "Id", "Name");
                return View("Create", new Question());
            }

            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            // Lọc và debug các câu hỏi hợp lệ
            var validQuestions = new List<Question>();
            foreach (var question in questions)
            {
                Console.WriteLine($"Received question: Content={question.Question_Content}, Subject={question.Subject_ID}, Level={question.Level_ID}, Type={question.QuestionTypeId}, CorrectOption={question.Correct_Option}, Options={string.Join(",", question.Options ?? new List<string>())}");

                if (!string.IsNullOrEmpty(question.Question_Content) && question.Subject_ID > 0 && question.Level_ID > 0 && question.QuestionTypeId > 0)
                {
                    validQuestions.Add(question);
                }
            }

            if (!validQuestions.Any())
            {
                ModelState.AddModelError("", "Không có câu hỏi hợp lệ để thêm.");
                Console.WriteLine("Không có câu hỏi hợp lệ.");
                ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
                ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName");
                ViewData["QuestionTypes"] = new SelectList(_context.QuestionType, "Id", "Name");
                return View("Create", new Question());
            }

            foreach (var question in validQuestions)
            {
                if (question.QuestionTypeId == 1) // Câu hỏi trắc nghiệm
                {
                    if (question.Options == null || question.Options.Count != 4 || question.Options.Any(opt => string.IsNullOrEmpty(opt)) || string.IsNullOrEmpty(question.Correct_Option))
                    {
                        Console.WriteLine($"Câu hỏi trắc nghiệm không hợp lệ: {question.Question_Content}");
                        ModelState.AddModelError($"Questions[{validQuestions.IndexOf(question)}].Options", "Vui lòng điền đầy đủ các đáp án và chọn đáp án đúng.");
                        continue;
                    }

                    if (!question.Options.Contains(question.Correct_Option))
                    {
                        Console.WriteLine($"Đáp án đúng không hợp lệ: CorrectOption={question.Correct_Option}, Options={string.Join(",", question.Options)}");
                        ModelState.AddModelError($"Questions[{validQuestions.IndexOf(question)}].CorrectOption", "Đáp án đúng không hợp lệ.");
                        continue;
                    }
                }
                else if (question.QuestionTypeId == 2) // Câu hỏi đúng/sai
                {
                    if (string.IsNullOrEmpty(question.Correct_Option) || (question.Correct_Option != "Đúng" && question.Correct_Option != "Sai"))
                    {
                        Console.WriteLine($"Câu hỏi đúng/sai không hợp lệ: {question.Question_Content}");
                        ModelState.AddModelError($"Questions[{validQuestions.IndexOf(question)}].CorrectOption", "Vui lòng chọn đáp án đúng/sai.");
                        continue;
                    }
                    question.Options = new List<string> { "Đúng", "Sai" };
                }
                else if (question.QuestionTypeId == 3) // Câu hỏi điền từ
                {
                    if (string.IsNullOrEmpty(question.Correct_Option))
                    {
                        Console.WriteLine($"Câu hỏi điền từ không hợp lệ: {question.Question_Content}");
                        ModelState.AddModelError($"Questions[{validQuestions.IndexOf(question)}].CorrectOption", "Vui lòng nhập đáp án đúng.");
                        continue;
                    }
                }

                question.CreatorUser_Id = teacherId;
                question.CreatedAt = DateTime.Now;

                try
                {
                    await _questionRepository.AddAsync(question);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm câu hỏi vào database: {ex.Message}");
                    ModelState.AddModelError("", $"Lỗi khi thêm câu hỏi: {ex.Message}");
                    ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
                    ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName");
                    ViewData["QuestionTypes"] = new SelectList(_context.QuestionType, "Id", "Name");
                    return View("Create", new Question());
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu vào database: {ex.Message}");
                ModelState.AddModelError("", $"Lỗi khi lưu vào database: {ex.Message}");
                ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
                ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName");
                ViewData["QuestionTypes"] = new SelectList(_context.QuestionType, "Id", "Name");
                return View("Create", new Question());
            }

            return RedirectToAction(nameof(Index));
        }
       
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] Dictionary<string, object> data)
        {
            int id = int.Parse(data["Question_ID"].ToString());
            var question = await _questionRepository.GetByIdAsync(id);
            if (question == null) return NotFound();

            question.Question_Content = data["Question_Content"]?.ToString();
            question.Correct_Option = data["Correct_Option"]?.ToString();
            question.Subject_ID = int.Parse(data["Subject_ID"].ToString());
            question.Level_ID = int.Parse(data["Level_ID"].ToString());

            // Xử lý options nếu có
            if (data.ContainsKey("Options") && data["Options"] != null)
            {
                question.Options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(data["Options"].ToString());
            }

            var userId = HttpContext.Session.GetString("UserId");
            question.CreatorUser_Id = int.Parse(userId);
            question.CreatedAt = DateTime.Now;
            await _questionRepository.UpdateAsync(question);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _questionRepository.GetByIdAsync(id);
            if (question == null)
                return NotFound();

            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            if (question.CreatorUser_Id != teacherId)
                return Unauthorized();

            await _questionRepository.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GenerateWithAI([FromBody] GenerateQuestionRequest request)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).Date;

            // Kiểm tra số lần sử dụng trong ngày
            var usageLog = await _context.AIUsageLog
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Date == today);

            if (usageLog != null && usageLog.UsageCount >= 6)
            {
                return BadRequest(new { error = "Bạn đã hết lượt sử dụng chức năng tạo câu hỏi với AI trong ngày hôm nay." });
            }

            // Nếu chưa có log thì tạo mới, nếu có thì tăng số lần sử dụng
            if (usageLog == null)
            {
                usageLog = new AIUsageLog
                {
                    UserId = userId,
                    Date = today,
                    UsageCount = 1
                };
                _context.AIUsageLog.Add(usageLog);
            }
            else
            {
                usageLog.UsageCount++;
                _context.AIUsageLog.Update(usageLog);
            }
            await _context.SaveChangesAsync();

            try
            {
                var subjectName = await _context.Subjects
                    .Where(s => s.Subject_Id == request.SubjectId)
                    .Select(s => s.Subject_Name)
                    .FirstOrDefaultAsync();

                var levelName = await _context.Levels
                    .Where(l => l.Id == request.LevelId)
                    .Select(l => l.LevelName)
                    .FirstOrDefaultAsync();

                var questionTypeName = await _context.QuestionType
                    .Where(qt => qt.Id == request.QuestionTypeId)
                    .Select(qt => qt.Name)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(subjectName) || string.IsNullOrEmpty(levelName) || string.IsNullOrEmpty(questionTypeName))
                {
                    return BadRequest(new { error = "Không tìm thấy thông tin tương ứng với ID." });
                }

                var noteText = string.IsNullOrWhiteSpace(request.Note) ? "" : $" ({request.Note})";
                var prompt = request.QuestionTypeId switch
                {
                    1 => $@"Tạo {request.NumberOfQuestions} câu hỏi trắc nghiệm về chủ đề '{subjectName}{noteText}' 
với độ khó '{levelName}'. Mỗi câu hỏi phải có đúng 4 đáp án, chỉ 1 đáp án đúng. 
Định dạng JSON như sau, đảm bảo tất cả các trường đều có giá trị:
[
    {{
        ""question"": ""Câu hỏi cụ thể"",
        ""options"": [""Đáp án 1"", ""Đáp án 2"", ""Đáp án 3"", ""Đáp án 4""],
        ""correctAnswer"": ""Đáp án đúng (phải nằm trong options)""
    }}
]",
                    2 => $@"Tạo {request.NumberOfQuestions} câu hỏi đúng/sai về chủ đề '{subjectName}{noteText}' 
với độ khó '{levelName}'. 
Định dạng JSON như sau, đảm bảo tất cả các trường đều có giá trị:
[
    {{
        ""question"": ""Câu hỏi cụ thể"",
        ""options"": [""Đúng"", ""Sai""],
        ""correctAnswer"": ""Đúng hoặc Sai""
    }}
]",
                    3 => $@"Tạo {request.NumberOfQuestions} câu hỏi điền từ về chủ đề '{subjectName}{noteText}' 
với độ khó '{levelName}'. 
Định dạng JSON như sau, đảm bảo tất cả các trường đều có giá trị:
[
    {{
        ""question"": ""Câu hỏi với chỗ trống cần điền"",
        ""correctAnswer"": ""Đáp án đúng""
    }}
]",
                    _ => throw new ArgumentException("Loại câu hỏi không hợp lệ.")
                };

                var aiResponse = await _chatbotService.GetResponseAsync(prompt);
                Console.WriteLine($"AI Raw Response: {aiResponse}");

                var jsonString = ExtractJsonFromResponse(aiResponse);
                Console.WriteLine($"Extracted JSON: {jsonString}");
                if (string.IsNullOrEmpty(jsonString))
                {
                    return BadRequest(new { error = "Không tìm thấy JSON hợp lệ trong phản hồi từ AI. Kiểm tra prompt hoặc phản hồi API." });
                }

                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                List<object> generatedQuestions;
                try
                {
                    switch (request.QuestionTypeId)
                    {
                        case 1:
                            generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestionMultipleChoice>>(jsonString, serializerOptions)?.Cast<object>().ToList() ?? new List<object>();
                            break;
                        case 2:
                            generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestionTrueFalse>>(jsonString, serializerOptions)?.Cast<object>().ToList() ?? new List<object>();
                            break;
                        case 3:
                            generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestionFillInTheBlank>>(jsonString, serializerOptions)?.Cast<object>().ToList() ?? new List<object>();
                            break;
                        default:
                            return BadRequest(new { error = "Loại câu hỏi không hợp lệ." });
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Deserialize Error: {ex.Message} - StackTrace: {ex.StackTrace}");
                    return BadRequest(new { error = "Lỗi khi phân tích JSON từ AI: Định dạng không hợp lệ." });
                }

                Console.WriteLine($"Số lượng câu hỏi sau deserialize: {generatedQuestions.Count}");

                if (!generatedQuestions.Any())
                {
                    Console.WriteLine("Danh sách câu hỏi từ AI rỗng sau khi deserialize.");
                    return BadRequest(new { error = "Phản hồi từ AI không hợp lệ hoặc không có câu hỏi nào được tạo." });
                }

                var questions = new List<object>();
                foreach (var q in generatedQuestions)
                {
                    Console.WriteLine($"Processing question of type: {q.GetType().Name}");
                    if (q is GeneratedQuestionMultipleChoice mc)
                    {
                        Console.WriteLine($"Options: {string.Join(",", mc.Options ?? new List<string>())}");
                        questions.Add(new
                        {
                            questionId = 0,
                            questionContent = mc.Question ?? "Không có nội dung câu hỏi",
                            options = mc.Options ?? new List<string> { "Đáp án 1", "Đáp án 2", "Đáp án 3", "Đáp án 4" },
                            correctOption = mc.CorrectAnswer ?? "Không có đáp án",
                            questionTypeId = request.QuestionTypeId,
                            isTrueFalse = false,
                            subjectId = request.SubjectId,
                            levelId = request.LevelId,
                            creatorUserId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0"),
                            createdAt = DateTime.Now 
                        });
                    }
                    else if (q is GeneratedQuestionTrueFalse tf)
                    {
                        Console.WriteLine($"Options: {string.Join(",", tf.Options ?? new List<string>())}");
                        questions.Add(new
                        {
                            questionId = 0,
                            questionContent = tf.Question ?? "Không có nội dung câu hỏi",
                            options = tf.Options ?? new List<string> { "Đúng", "Sai" },
                            correctOption = tf.CorrectAnswer ?? "Không có đáp án",
                            questionTypeId = request.QuestionTypeId,
                            isTrueFalse = true,
                            subjectId = request.SubjectId,
                            levelId = request.LevelId,
                            creatorUserId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0"),
                            createdAt = DateTime.Now
                        });
                    }
                    else if (q is GeneratedQuestionFillInTheBlank fb)
                    {
                        Console.WriteLine("No options for FillInTheBlank type");
                        questions.Add(new
                        {
                            questionId = 0,
                            questionContent = fb.Question ?? "Không có nội dung câu hỏi",
                            options = (List<string>)null,
                            correctOption = fb.CorrectAnswer ?? "Không có đáp án",
                            questionTypeId = request.QuestionTypeId,
                            isTrueFalse = false,
                            subjectId = request.SubjectId,
                            levelId = request.LevelId,
                            creatorUserId = int.Parse(HttpContext.Session.GetString("UserId") ?? "0"),
                            createdAt = DateTime.Now
                        });
                    }
                }

                Console.WriteLine($"Số lượng câu hỏi sau ánh xạ: {questions.Count}");

                if (!questions.Any())
                {
                    Console.WriteLine("Danh sách câu hỏi rỗng sau khi ánh xạ. Kiểm tra kiểu dữ liệu: " + string.Join(", ", generatedQuestions.Select(q => q?.GetType().Name ?? "null")));
                    return BadRequest(new { error = "Phản hồi từ AI không hợp lệ hoặc không có câu hỏi nào được tạo." });
                }

                return Json(questions, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }, 
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Error: {ex.Message} - StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = $"Lỗi khi phân tích JSON từ AI: {ex.Message}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message} - StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = $"Lỗi khi tạo câu hỏi với AI: {ex.Message}" });
            }
        }

        private string ExtractJsonFromResponse(string response)
        {
            if (string.IsNullOrEmpty(response)) return null;

            try
            {
                // Parse response as JSON to check structure
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(response);
                if (jsonResponse.TryGetProperty("candidates", out JsonElement candidates))
                {
                    if (candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];
                        if (candidate.TryGetProperty("content", out JsonElement content) &&
                            content.TryGetProperty("parts", out JsonElement parts))
                        {
                            var text = parts[0].GetProperty("text").GetString();
                            // Attempt to extract JSON from text if it contains JSON
                            var jsonMatch = Regex.Match(text, @"\[\s*\{.*?\}\s*\]", RegexOptions.Singleline);
                            if (jsonMatch.Success)
                            {
                                return jsonMatch.Value.Trim();
                            }
                        }
                    }
                }
                // Fallback to return the entire response if it looks like JSON array
                if (response.TrimStart().StartsWith("[") && response.TrimEnd().EndsWith("]"))
                {
                    return response.Trim();
                }
                Console.WriteLine("Không tìm thấy JSON hợp lệ trong phản hồi: " + response);
                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message} - Response: {response}");
                return null;
            }
        }



    }
}
