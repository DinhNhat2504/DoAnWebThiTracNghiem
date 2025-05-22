using DoAnWebThiTracNghiem.Models;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.Services;
using DoAnWebThiTracNghiem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DoAnWebThiTracNghiem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuestionsController : Controller
    {
        public readonly IQuestionRepository _Qcontext;
        private readonly IAIUsageLogRepository _aiUsageLogRepository;
        private readonly ChatbotService _chatbotService;
        public QuestionsController(IQuestionRepository qcontext, IAIUsageLogRepository aiUsageLogRepository, ChatbotService chatbotService)
        {
            _Qcontext = qcontext;
            _aiUsageLogRepository = aiUsageLogRepository;
            _chatbotService = chatbotService;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string search = "", int? subjectId = null, int? levelId = null, int? questionTypeId = null)
        {
            var UserId = HttpContext.Session.GetString("UserId");
            var RoleId = HttpContext.Session.GetString("RoleId");
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(RoleId))
            {
                return RedirectToAction("Index", "Home");
            }
            int userId = int.Parse(UserId);
            int roleId = int.Parse(RoleId);
            var subjects = await _Qcontext.GetAllSubjectsAsync(1, userId);
            var levels = await _Qcontext.GetAllLevelsAsync();
            var questionTypes = await _Qcontext.GetAllQuestionTypesAsync();
            // Sửa lại repository để nhận thêm các tham số lọc
            var total = await _Qcontext.CountAsync(roleId, userId, search, subjectId, levelId, questionTypeId);
            var items = await _Qcontext.GetPagedAsync(roleId, userId, page, pageSize, search, subjectId, levelId, questionTypeId);

            var model = new PagedResult<Question>
            {
                Items = items,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                TotalItems = total,
                Search = search,
                SubjectId = subjectId,
                LevelId = levelId,
                QuestionTypeId = questionTypeId
            };

            ViewData["Subjects"] = new SelectList(
            subjects.Select(s => new
            {
            Subject_Id = s.Subject_Id,
            Subject_Display = $"{s.Subject_Name} - {s.CreatorName}"
            }),
            "Subject_Id",
            "Subject_Display",
                subjectId // Giá trị mặc định được chọn
                );

            ViewData["Levels"] = new SelectList(levels, "Id", "LevelName", levelId);
            ViewData["QuestionTypes"] = new SelectList(questionTypes, "Id", "Name", questionTypeId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                int adminId = int.Parse(userId);
                question.CreatorUser_Id = adminId;
                await _Qcontext.AddAsync(question);
                return RedirectToAction(nameof(Index));
            }
            return View(question);
        }
        [HttpPost]
        public async Task<IActionResult> CreateMultiple(List<Question> questions)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int adminId = int.Parse(userId);
            var subjects = await _Qcontext.GetAllSubjectsAsync(1, adminId);
            var levels = await _Qcontext.GetAllLevelsAsync();
            var questionTypes = await _Qcontext.GetAllQuestionTypesAsync();
            if (questions == null || !questions.Any())
            {
                ModelState.AddModelError("", "Không có câu hỏi nào để thêm.");
                Console.WriteLine("Không có câu hỏi nào để thêm.");
                ViewData["Subjects"] = new SelectList(subjects, "Subject_Id", "Subject_Name");
                ViewData["Levels"] = new SelectList(levels, "Id", "LevelName");
                ViewData["QuestionTypes"] = new SelectList(questionTypes, "Id", "Name");
                return View("Create", new Question());
            }



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
                ViewData["Subjects"] = new SelectList(subjects, "Subject_Id", "Subject_Name");
                ViewData["Levels"] = new SelectList(levels, "Id", "LevelName");
                ViewData["QuestionTypes"] = new SelectList(questionTypes, "Id", "Name");
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

                question.CreatorUser_Id = adminId;
                question.CreatedAt = DateTime.Now;

                try
                {
                    await _Qcontext.AddAsync(question);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm câu hỏi vào database: {ex.Message}");
                    ModelState.AddModelError("", $"Lỗi khi thêm câu hỏi: {ex.Message}");
                    ViewData["Subjects"] = new SelectList(subjects, "Subject_Id", "Subject_Name");
                    ViewData["Levels"] = new SelectList(levels, "Id", "LevelName");
                    ViewData["QuestionTypes"] = new SelectList(questionTypes, "Id", "Name");
                    return View("Create", new Question());
                }
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> GetQuesion(int id)
        {
            var question = await _Qcontext.GetByIdAsync(id);
            if (question == null) return NotFound();
            return Json(question);
        }
        public async Task<IActionResult> GetSubjectForQuestion(int questionId)
        {
            var question = await _Qcontext.GetByIdAsync(questionId);
            if (question == null)
                return Json(new List<Subject>());
            var subjects = await _Qcontext.GetSubjectsByCreatorAsync(question.CreatorUser_Id);
            return Json(subjects ?? new List<Subject>());
        }
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int adminId = int.Parse(userId);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(HttpContext.Session.GetString("RoleId")))
            {
                return RedirectToAction("Index", "Home");
            }
            var subjects = await _Qcontext.GetAllSubjectsAsync(1, adminId);
            var levels = await _Qcontext.GetAllLevelsAsync();
            var questionTypes = await _Qcontext.GetAllQuestionTypesAsync();

            ViewData["Subjects"] = new SelectList(subjects, "Subject_Id", "Subject_Name");
            ViewData["Levels"] = new SelectList(levels, "Id", "LevelName");
            ViewData["QuestionTypes"] = new SelectList(questionTypes, "Id", "Name");

            return View(new Question());
        }
        [HttpPost]
        public async Task<IActionResult> GenerateWithAI([FromBody] GenerateQuestionRequest request)
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).Date;

            // Kiểm tra số lần sử dụng trong ngày
            var usageLog = await _aiUsageLogRepository.GetByUserAndDateAsync(userId, today);

            if (usageLog != null && usageLog.UsageCount >= 6)
            {
                return BadRequest(new { error = "Bạn đã hết lượt sử dụng chức năng tạo câu hỏi với AI trong ngày hôm nay." });
            }

            if (usageLog == null)
            {
                usageLog = new AIUsageLog
                {
                    UserId = userId,
                    Date = today,
                    UsageCount = 1
                };
                await _aiUsageLogRepository.AddAsync(usageLog);
            }
            else
            {
                usageLog.UsageCount++;
                await _aiUsageLogRepository.UpdateAsync(usageLog);
            }
            await _aiUsageLogRepository.SaveChangesAsync();

            try
            {
                var subjectName = await _Qcontext.GetSubjectNameByIdAsync(request.SubjectId);

                var levelName = await _Qcontext.GetLevelNameByIdAsync(request.LevelId);

                var questionTypeName = await _Qcontext.GetQTypeNameByIdAsync(request.QuestionTypeId);

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
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] Dictionary<string, object> data)
        {
            int id = int.Parse(data["Question_ID"].ToString());
            var question = await _Qcontext.GetByIdAsync(id);
            if (question == null) return NotFound();

            var errors = new Dictionary<string, string[]>();

            // Validate Question_Content
            var content = data["Question_Content"]?.ToString();
            if (string.IsNullOrWhiteSpace(content))
                errors["Question_Content"] = new[] { "Nội dung câu hỏi là bắt buộc" };
            else if (content.Length > 1000)
                errors["Question_Content"] = new[] { "Nội dung câu hỏi không được vượt quá 1000 ký tự" };

            // Validate Correct_Option
            var correctOption = data["Correct_Option"]?.ToString();
            if (string.IsNullOrWhiteSpace(correctOption))
                errors["Correct_Option"] = new[] { "Đáp án đúng là bắt buộc" };
            else if (correctOption.Length > 500)
                errors["Correct_Option"] = new[] { "Đáp án đúng không được vượt quá 500 ký tự" };

            // Validate Subject_ID
            if (!int.TryParse(data["Subject_ID"]?.ToString(), out int subjectId) || subjectId < 1)
                errors["Subject_ID"] = new[] { "Vui lòng chọn môn học hợp lệ" };

            // Validate Level_ID
            if (!int.TryParse(data["Level_ID"]?.ToString(), out int levelId) || levelId < 1)
                errors["Level_ID"] = new[] { "Vui lòng chọn mức độ hợp lệ" };

            // Validate Options nếu có
            List<string>? options = null;
            if (data.ContainsKey("Options") && data["Options"] != null)
            {
                try
                {
                    options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(data["Options"].ToString());
                }
                catch
                {
                    errors["Options"] = new[] { "Định dạng đáp án không hợp lệ" };
                }
            }

            // Validate theo loại câu hỏi (nếu có QuestionTypeId)
            int questionTypeId = question.QuestionTypeId;
            if (data.ContainsKey("QuestionTypeId") && int.TryParse(data["QuestionTypeId"]?.ToString(), out int qTypeId))
                questionTypeId = qTypeId;

            if (questionTypeId == 1) // Trắc nghiệm 1 đáp án
            {
                if (options == null || options.Count != 4 || options.Any(string.IsNullOrWhiteSpace))
                    errors["Options"] = new[] { "Vui lòng điền đủ 4 đáp án và không để trống." };
                if (options != null && !options.Contains(correctOption))
                    errors["Correct_Option"] = new[] { "Đáp án đúng phải nằm trong các đáp án đã nhập." };
            }
            else if (questionTypeId == 2) // Đúng/Sai
            {
                if (correctOption != "Đúng" && correctOption != "Sai")
                    errors["Correct_Option"] = new[] { "Đáp án đúng phải là 'Đúng' hoặc 'Sai'." };
            }
            else if (questionTypeId == 3) // Điền từ
            {
                if (string.IsNullOrWhiteSpace(correctOption))
                    errors["Correct_Option"] = new[] { "Vui lòng nhập đáp án đúng." };
            }

            if (errors.Count > 0)
                return BadRequest(new { success = false, errors });

            // Nếu hợp lệ, cập nhật dữ liệu
            question.Question_Content = content;
            question.Correct_Option = correctOption;
            question.Subject_ID = subjectId;
            question.Level_ID = levelId;
            if (options != null)
                question.Options = options;

            question.CreatedAt = DateTime.Now;
            await _Qcontext.UpdateAsync(question);
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _Qcontext.GetByIdAsync(id);
            if (question == null)
                return NotFound();

            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);
            if (question.CreatorUser_Id != teacherId)
                return Unauthorized();

            await _Qcontext.DeleteAsync(id);

            return Ok();
        }
    }
}
