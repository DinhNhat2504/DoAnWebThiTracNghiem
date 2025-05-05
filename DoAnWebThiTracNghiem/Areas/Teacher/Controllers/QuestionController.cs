using Azure;
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
using X.PagedList.Extensions;

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
        public async Task<IActionResult> Index(string searchString, int? subjectId, int? page1, int? page2, int? page3)
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            var questions = await _questionRepository.GetAllAsync(teacherId);

            // Lọc theo tên câu hỏi
            if (!string.IsNullOrEmpty(searchString))
                questions = questions.Where(q => q.Question_Content.Contains(searchString, StringComparison.OrdinalIgnoreCase));

            // Lọc theo môn học
            if (subjectId.HasValue && subjectId.Value > 0)
                questions = questions.Where(q => q.Subject_ID == subjectId.Value);

            // Phân loại câu hỏi
            var singleAnswerQuestions = questions.Where(q => q.QuestionTypeId == 1);
            var trueFalseQuestions = questions.Where(q => q.QuestionTypeId == 2);
            var fillInTheBlankQuestions = questions.Where(q => q.QuestionTypeId == 3);

            int pageSize = 5;
            int pageNumber1 = page1 ?? 1;
            int pageNumber2 = page2 ?? 1;
            int pageNumber3 = page3 ?? 1;

            ViewData["SingleAnswerQuestions"] = singleAnswerQuestions.ToPagedList(pageNumber1, pageSize);
            ViewData["TrueFalseQuestions"] = trueFalseQuestions.ToPagedList(pageNumber2, pageSize);
            ViewData["FillInTheBlankQuestions"] = fillInTheBlankQuestions.ToPagedList(pageNumber3, pageSize);

            // Để render dropdown môn học
            ViewData["Subjects"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
            ViewData["Levels"] = new SelectList(_context.Levels, "Id", "LevelName");


            return View();
        }



        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int teacherId = int.Parse(userId);

            ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
            ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName");
            ViewData["QuestionTypes"] = new SelectList(_context.QuestionType, "Id", "Name");

            return View(new Question());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Question question, Dictionary<string, string> Options, string CorrectOption)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == int.Parse(HttpContext.Session.GetString("UserId"))), "Subject_Id", "Subject_Name");
                ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName");
                ViewData["QuestionTypes"] = new SelectList(_context.QuestionType, "Id", "Name");
                return View(question);
            }

            var userID = HttpContext.Session.GetString("UserId");
            question.CreatorUser_Id = int.Parse(userID);
            question.CreatedAt = DateTime.Now;

            // Xử lý theo loại câu hỏi
            if (question.QuestionTypeId == 1) // Câu hỏi 1 đáp án
            {
                question.Options = Options.Values.ToList();
                // Tìm key trong Options có giá trị khớp với CorrectOption
                var correctOptionKey = Options.FirstOrDefault(x => x.Value == CorrectOption).Key;
                if (!string.IsNullOrEmpty(correctOptionKey))
                {
                    question.Correct_Option = CorrectOption; // Lưu nội dung đáp án (như "rrrrrr")
                }
                else
                {
                    ModelState.AddModelError("CorrectOption", "Đáp án đúng không hợp lệ.");
                    ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == int.Parse(HttpContext.Session.GetString("UserId"))), "Subject_Id", "Subject_Name");
                    ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName");
                    ViewData["QuestionTypes"] = new SelectList(_context.QuestionType, "Id", "Name");
                    return View(question);
                }
            }
            else if (question.QuestionTypeId == 2) // Câu hỏi đúng/sai
            {
                question.Options = new List<string> { "Đúng", "Sai" };
                question.Correct_Option = CorrectOption;
            }
            else if (question.QuestionTypeId == 3) // Câu hỏi điền từ
            {
                question.Correct_Option = CorrectOption;
            }

            await _questionRepository.AddAsync(question);
            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _questionRepository.GetByIdAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            var userId = HttpContext.Session.GetString("UserId");
            var teacherId = int.Parse(userId);
            if (question.CreatorUser_Id != teacherId)
            {
                return Unauthorized();
            }
            ViewData["SubjectId"] = new SelectList(_context.Subjects.Where(s => s.CreatorUser_Id == teacherId), "Subject_Id", "Subject_Name");
            ViewData["LevelId"] = new SelectList(_context.Levels, "Level_ID", "Level_Name", question.Level_ID);
            return View(question);
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


        public async Task<IActionResult> CreateWithAI()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account"); // Handle missing session
            }
            int teacherId = int.Parse(userId);

            var level = await _context.Levels.ToListAsync();
            var subject = await _context.Subjects
                .Where(s => s.CreatorUser_Id == teacherId)
                .ToListAsync();
            var questiont = await _context.QuestionType.ToListAsync();

            ViewData["Subjects"] = subject;
            ViewData["Levels"] = level;
            ViewData["QuestionTypes"] = questiont;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateWithAI([FromBody] GenerateQuestionRequest request)
        {
            if (request.NumberOfQuestions <= 0 || request.SubjectId <= 0 || request.LevelId <= 0 || request.QuestionTypeId <= 0)
            {
                return BadRequest(new { error = "Thông tin không hợp lệ." });
            }

            try
            {
                // Lấy tên môn học, mức độ và loại câu hỏi từ cơ sở dữ liệu
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

                // Tạo prompt cho AI
                var prompt = request.QuestionTypeId switch
                {
                    1 => $@"Tạo {request.NumberOfQuestions} câu hỏi trắc nghiệm về chủ đề '{subjectName}' 
với độ khó '{levelName}'. Mỗi câu hỏi phải có đúng 4 đáp án, chỉ 1 đáp án đúng. 
Định dạng JSON như sau, đảm bảo tất cả các trường đều có giá trị:
[
    {{
        ""question"": ""Câu hỏi cụ thể"",
        ""options"": [""Đáp án 1"", ""Đáp án 2"", ""Đáp án 3"", ""Đáp án 4""],
        ""correctAnswer"": ""Đáp án đúng (phải nằm trong options)""
    }}
]",

                    2 => $@"Tạo {request.NumberOfQuestions} câu hỏi đúng/sai về chủ đề '{subjectName}' 
với độ khó '{levelName}'. 
Định dạng JSON như sau, đảm bảo tất cả các trường đều có giá trị:
[
    {{
        ""question"": ""Câu hỏi cụ thể"",
        ""options"": [""Đúng"", ""Sai""],
        ""correctAnswer"": ""Đúng hoặc Sai""
    }}
]",

                    3 => $@"Tạo {request.NumberOfQuestions} câu hỏi điền từ về chủ đề '{subjectName}' 
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

                // Gửi prompt đến AI
                var aiResponse = await _chatbotService.GetResponseAsync(prompt);
                Console.WriteLine($"AI Raw Response: {aiResponse}"); // Log phản hồi thô từ AI

                // Làm sạch JSON từ phản hồi
                var jsonString = ExtractJsonFromResponse(aiResponse);
                Console.WriteLine($"Extracted JSON: {jsonString}"); // Log JSON đã làm sạch
                if (string.IsNullOrEmpty(jsonString))
                {
                    return BadRequest(new { error = "Không tìm thấy JSON hợp lệ trong phản hồi từ AI." });
                }

                // Parse phản hồi từ AI
                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Hỗ trợ tiếng Việt
                };

                // Deserialize dựa trên loại câu hỏi
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
                            throw new ArgumentException("Loại câu hỏi không hợp lệ.");
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Deserialize Error: {ex.Message} - StackTrace: {ex.StackTrace}");
                    return BadRequest(new { error = "Lỗi khi phân tích JSON từ AI: Định dạng không hợp lệ." });
                }

                // Log số lượng câu hỏi sau khi deserialize
                Console.WriteLine($"Số lượng câu hỏi sau deserialize: {generatedQuestions.Count}");

                if (!generatedQuestions.Any())
                {
                    Console.WriteLine("Danh sách câu hỏi từ AI rỗng sau khi deserialize.");
                    return BadRequest(new { error = "Phản hồi từ AI không hợp lệ hoặc không có câu hỏi nào được tạo." });
                }

                // Ánh xạ thủ công dựa trên loại câu hỏi
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

                // Log số lượng câu hỏi sau khi ánh xạ
                Console.WriteLine($"Số lượng câu hỏi sau ánh xạ: {questions.Count}");

                if (!questions.Any())
                {
                    Console.WriteLine("Danh sách câu hỏi rỗng sau khi ánh xạ. Kiểm tra kiểu dữ liệu: " + string.Join(", ", generatedQuestions.Select(q => q?.GetType().Name ?? "null")));
                    return BadRequest(new { error = "Phản hồi từ AI không hợp lệ hoặc không có câu hỏi nào được tạo." });
                }

                // Trả về JSON với camelCase
                return Json(questions, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() },
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Hỗ trợ tiếng Việt
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

        // Hàm trích xuất JSON từ phản hồi
        private string ExtractJsonFromResponse(string response)
        {
            if (string.IsNullOrEmpty(response)) return null;

            // Sử dụng regex để trích xuất JSON
            var jsonMatch = Regex.Match(response, @"\[\s*\{.*?\}\s*\]", RegexOptions.Singleline);
            if (jsonMatch.Success)
            {
                return jsonMatch.Value.Trim();
            }

            // Nếu không tìm thấy, kiểm tra xem toàn bộ response có phải JSON không
            if (response.TrimStart().StartsWith("[") && response.TrimEnd().EndsWith("]"))
            {
                return response.Trim();
            }

            // Log nếu không tìm thấy JSON
            Console.WriteLine("Không tìm thấy JSON hợp lệ trong phản hồi: " + response);
            return null;
        }



    }
}
