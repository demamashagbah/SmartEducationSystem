using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEducation.Application.Constants;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Domain.Entities;
using SmartEducation.Web.Areas.Admin.Models;

namespace SmartEducation.Web.Areas.Admin.Contollers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class SubjectController : Controller
    {
        private readonly ISubjectService _subjectService;
        private readonly ITeacherGuideService _teacherGuideService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;

        public SubjectController(ISubjectService subjectService, ITeacherGuideService teacherGuideService,
            IUnitOfWork unitOfWork, IWebHostEnvironment env)
        {
            _subjectService = subjectService;
            _teacherGuideService = teacherGuideService;
            _unitOfWork = unitOfWork;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var subject = await _subjectService.GetByIdAsync(id);
            if (subject == null) return NotFound();
            return View(new SubjectViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description,
                ClassRoomId = subject.ClassRoomId ?? Guid.Empty,
                ClassRoomName = subject.ClassRoomName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubjectViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            await _subjectService.UpdateAsync(new SubjectDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                ClassRoomId = model.ClassRoomId
            });
            TempData["Success"] = "Subject updated successfully.";
            return RedirectToAction("Manage", "ClassRoom", new { id = model.ClassRoomId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var curriculum = await _teacherGuideService.GetSubjectCurriculumAsync(id);
            if (curriculum == null || curriculum.SubjectId == Guid.Empty) return NotFound();
            return View(curriculum);
        }

        [HttpGet]
        public async Task<IActionResult> UploadGuide(Guid id)
        {
            var subject = await _subjectService.GetByIdAsync(id);
            if (subject == null) return NotFound();
            ViewBag.SubjectId = id;
            ViewBag.SubjectName = subject.Name;
            ViewBag.ClassRoomId = subject.ClassRoomId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadGuide(Guid subjectId, IFormFile file, string? description)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                ViewBag.SubjectId = subjectId;
                return View();
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "teacher-guides");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var relativePath = $"/uploads/teacher-guides/{uniqueName}";
            await _teacherGuideService.UploadGuideAsync(subjectId, file.FileName, relativePath, description);

            TempData["Success"] = $"Teacher Guide '{file.FileName}' uploaded successfully.";
            return RedirectToAction(nameof(Details), new { id = subjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGuide(Guid guideId, Guid subjectId)
        {
            await _teacherGuideService.DeleteGuideAsync(guideId);
            TempData["Success"] = "Teacher Guide deleted.";
            return RedirectToAction(nameof(Details), new { id = subjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExtractCurriculum(Guid guideId, Guid subjectId)
        {
            var guide = await _unitOfWork.TeacherGuides.GetByIdAsync(guideId);
            var subject = await _subjectService.GetByIdAsync(subjectId);
            if (guide == null || subject == null)
            {
                TempData["Error"] = "Guide or subject not found.";
                return RedirectToAction(nameof(Details), new { id = subjectId });
            }

            var existingUnits = await _unitOfWork.Units.GetAllAsync();
            if (existingUnits.Any(u => u.SubjectId == subjectId))
            {
                TempData["Error"] = "This subject already has curriculum content. Clear existing units first to re-extract.";
                return RedirectToAction(nameof(Details), new { id = subjectId });
            }

            var curriculum = GenerateCurriculumFromGuide(subject.Name, guide.FileName, guide.Description);
            foreach (var unitData in curriculum)
            {
                var unit = new Domain.Entities.Unit { Id = Guid.NewGuid(), Name = unitData.Name, SubjectId = subjectId };
                await _unitOfWork.Units.AddAsync(unit);
                await _unitOfWork.SaveChangesAsync();

                foreach (var lessonData in unitData.Lessons)
                {
                    var lesson = new Lesson { Id = Guid.NewGuid(), Name = lessonData.Name, UnitId = unit.Id };
                    await _unitOfWork.Lessons.AddAsync(lesson);
                    await _unitOfWork.SaveChangesAsync();

                    foreach (var topicData in lessonData.Topics)
                    {
                        var topic = new Topic { Id = Guid.NewGuid(), Name = topicData.Name, LessonId = lesson.Id };
                        await _unitOfWork.Topics.AddAsync(topic);
                        await _unitOfWork.SaveChangesAsync();

                        foreach (var outcome in topicData.LearningOutcomes)
                        {
                            await _unitOfWork.LearningOutcomes.AddAsync(new LearningOutcome
                            {
                                Id = Guid.NewGuid(),
                                TopicId = topic.Id,
                                Description = outcome
                            });
                        }
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }

            guide.IsAnalyzed = true;
            guide.AnalysisNotes = $"AI extracted {curriculum.Count} units on {DateTime.UtcNow:MMM dd, yyyy}.";
            await _unitOfWork.TeacherGuides.UpdateAsync(guide);
            await _unitOfWork.SaveChangesAsync();

            int totalUnits    = curriculum.Count;
            int totalLessons  = curriculum.Sum(u => u.Lessons.Count);
            int totalTopics   = curriculum.Sum(u => u.Lessons.Sum(l => l.Topics.Count));
            int totalOutcomes = curriculum.Sum(u => u.Lessons.Sum(l => l.Topics.Sum(t => t.LearningOutcomes.Count)));

            TempData["Success"] = $"AI successfully extracted curriculum: {totalUnits} units, {totalLessons} lessons, {totalTopics} topics, {totalOutcomes} learning outcomes.";
            return RedirectToAction(nameof(Details), new { id = subjectId });
        }

        private static List<UnitSeed> GenerateCurriculumFromGuide(string subjectName, string fileName, string? description)
        {
            var name = subjectName.ToLower();

            if (name.Contains("math") || name.Contains("algebra") || name.Contains("calculus") || name.Contains("geometry"))
                return MathCurriculum();
            if (name.Contains("science") || name.Contains("biology") || name.Contains("chemistry") || name.Contains("physics"))
                return ScienceCurriculum();
            if (name.Contains("english") || name.Contains("literature") || name.Contains("reading") || name.Contains("writing"))
                return EnglishCurriculum();
            if (name.Contains("arabic") || name.Contains("عربي") || name.Contains("لغة"))
                return ArabicCurriculum();
            if (name.Contains("history") || name.Contains("social") || name.Contains("geography") || name.Contains("civic"))
                return SocialStudiesCurriculum(subjectName);
            if (name.Contains("computer") || name.Contains("technology") || name.Contains("ict") || name.Contains("programming"))
                return ComputingCurriculum();
            if (name.Contains("islamic") || name.Contains("religion") || name.Contains("quran") || name.Contains("دين"))
                return IslamicStudiesCurriculum();

            return GenericCurriculum(subjectName);
        }

        private static List<UnitSeed> MathCurriculum() => new()
        {
            new("Unit 1: Number Systems & Operations", new()
            {
                new("Natural Numbers and Integers", new()
                {
                    new("Properties of Natural Numbers", ["Identify and apply properties of natural numbers", "Compare and order integers on a number line"]),
                    new("Integer Operations", ["Perform addition and subtraction of integers", "Multiply and divide integers using correct sign rules"])
                }),
                new("Fractions and Decimals", new()
                {
                    new("Equivalent Fractions", ["Simplify fractions to their lowest terms", "Convert between fractions and decimals"]),
                    new("Operations with Fractions", ["Add and subtract fractions with unlike denominators", "Multiply and divide fractions and mixed numbers"])
                }),
                new("Ratio and Proportion", new()
                {
                    new("Understanding Ratios", ["Write and interpret ratios in different forms", "Solve problems involving equivalent ratios"]),
                    new("Direct and Inverse Proportion", ["Identify direct and inverse proportional relationships", "Solve real-world proportion problems"])
                })
            }),
            new("Unit 2: Algebra & Equations", new()
            {
                new("Introduction to Algebra", new()
                {
                    new("Variables and Expressions", ["Write algebraic expressions from word problems", "Evaluate algebraic expressions by substitution"]),
                    new("Simplifying Expressions", ["Combine like terms to simplify expressions", "Apply the distributive property"])
                }),
                new("Linear Equations", new()
                {
                    new("Solving One-Step Equations", ["Solve linear equations using inverse operations", "Verify solutions by substitution"]),
                    new("Solving Multi-Step Equations", ["Solve two-step and multi-step linear equations", "Formulate equations from word problems"])
                }),
                new("Linear Inequalities", new()
                {
                    new("Understanding Inequalities", ["Graph inequalities on a number line", "Write and interpret inequality notation"]),
                    new("Solving Inequalities", ["Solve linear inequalities and graph the solution", "Apply inequalities to real-world situations"])
                })
            }),
            new("Unit 3: Geometry & Measurement", new()
            {
                new("Plane Figures", new()
                {
                    new("Polygons and Properties", ["Classify polygons by sides and angles", "Calculate perimeter of regular and irregular polygons"]),
                    new("Area of Plane Figures", ["Calculate area of triangles, rectangles, and parallelograms", "Decompose complex shapes to find total area"])
                }),
                new("Solid Geometry", new()
                {
                    new("3D Shapes", ["Identify faces, edges, and vertices of 3D solids", "Calculate surface area of cubes and cuboids"]),
                    new("Volume", ["Calculate volume of cubes and rectangular prisms", "Solve problems involving capacity and volume"])
                }),
                new("Statistics and Data", new()
                {
                    new("Data Collection and Display", ["Collect and organize data in tables and charts", "Read and interpret bar graphs, line graphs, and pie charts"]),
                    new("Measures of Central Tendency", ["Calculate mean, median, and mode of data sets", "Choose the appropriate measure of central tendency for a given context"])
                })
            })
        };

        private static List<UnitSeed> ScienceCurriculum() => new()
        {
            new("Unit 1: Living Things and Their Environment", new()
            {
                new("Cells: The Building Blocks of Life", new()
                {
                    new("Cell Structure", ["Identify the major components of plant and animal cells", "Compare prokaryotic and eukaryotic cell structures"]),
                    new("Cell Functions", ["Describe the role of organelles in cellular activities", "Explain how cells maintain homeostasis"])
                }),
                new("Classification of Living Organisms", new()
                {
                    new("The Five Kingdoms", ["Classify organisms into kingdoms based on characteristics", "Use a dichotomous key to identify species"]),
                    new("Ecosystems and Food Chains", ["Construct food chains and food webs in ecosystems", "Describe energy flow through trophic levels"])
                }),
                new("Human Body Systems", new()
                {
                    new("Digestive and Respiratory Systems", ["Trace the path of food through the digestive system", "Describe gas exchange in the respiratory system"]),
                    new("Circulatory and Nervous Systems", ["Explain the structure and function of the heart and blood vessels", "Describe how the nervous system coordinates body responses"])
                })
            }),
            new("Unit 2: Matter and Its Properties", new()
            {
                new("States of Matter", new()
                {
                    new("Properties of Solids, Liquids, and Gases", ["Describe the particle arrangement in each state of matter", "Explain how temperature affects states of matter"]),
                    new("Changes of State", ["Identify and explain melting, freezing, evaporation, and condensation", "Apply the concept of latent heat to phase changes"])
                }),
                new("Elements, Compounds, and Mixtures", new()
                {
                    new("Pure Substances vs Mixtures", ["Distinguish between elements, compounds, and mixtures", "Describe methods to separate common mixtures"]),
                    new("Atomic Structure", ["Describe the structure of an atom including protons, neutrons, and electrons", "Use the periodic table to identify element properties"])
                }),
                new("Chemical Reactions", new()
                {
                    new("Types of Chemical Reactions", ["Identify synthesis, decomposition, and displacement reactions", "Write balanced chemical equations"]),
                    new("Acids, Bases, and pH", ["Use pH scale to classify solutions as acid or base", "Describe the properties and uses of common acids and bases"])
                })
            }),
            new("Unit 3: Forces and Energy", new()
            {
                new("Forces and Motion", new()
                {
                    new("Newton's Laws", ["State and apply Newton's three laws of motion", "Calculate force, mass, and acceleration using F = ma"]),
                    new("Types of Forces", ["Describe gravitational, frictional, and applied forces", "Analyze balanced and unbalanced forces in diagrams"])
                }),
                new("Energy and Its Forms", new()
                {
                    new("Types of Energy", ["Identify kinetic and potential energy in real-world contexts", "Describe transformations between different forms of energy"]),
                    new("Thermal Energy and Heat Transfer", ["Explain conduction, convection, and radiation", "Apply principles of heat transfer to everyday situations"])
                }),
                new("Waves and Light", new()
                {
                    new("Properties of Waves", ["Identify amplitude, wavelength, and frequency of waves", "Distinguish between transverse and longitudinal waves"]),
                    new("Light and Optics", ["Explain reflection and refraction of light", "Describe how lenses and mirrors form images"])
                })
            })
        };

        private static List<UnitSeed> EnglishCurriculum() => new()
        {
            new("Unit 1: Reading & Comprehension", new()
            {
                new("Fiction and Narrative Texts", new()
                {
                    new("Story Elements", ["Identify plot, setting, characters, and theme in a narrative", "Analyze how the author develops characters through dialogue and action"]),
                    new("Reading Strategies", ["Apply skimming and scanning strategies to locate information", "Make inferences and draw conclusions from text"])
                }),
                new("Non-Fiction and Informational Texts", new()
                {
                    new("Text Structure", ["Identify organizational patterns: cause-effect, compare-contrast, sequence", "Use headings, subheadings, and captions to comprehend informational text"]),
                    new("Critical Reading", ["Distinguish facts from opinions in informational texts", "Evaluate the author's purpose and point of view"])
                }),
                new("Poetry and Literary Devices", new()
                {
                    new("Poetic Forms", ["Identify and analyze features of different poem types", "Understand rhyme scheme, rhythm, and meter"]),
                    new("Figurative Language", ["Identify simile, metaphor, personification, and alliteration", "Analyze how figurative language enhances meaning"])
                })
            }),
            new("Unit 2: Writing Skills", new()
            {
                new("The Writing Process", new()
                {
                    new("Planning and Drafting", ["Use brainstorming and outlining strategies to plan writing", "Write a first draft with a clear introduction, body, and conclusion"]),
                    new("Editing and Proofreading", ["Revise writing for clarity, organization, and style", "Proofread for grammar, punctuation, and spelling errors"])
                }),
                new("Expository Writing", new()
                {
                    new("Paragraph Development", ["Write well-structured paragraphs with topic sentences and supporting details", "Use transition words to connect ideas"]),
                    new("Essay Writing", ["Write a five-paragraph essay on a given topic", "Support arguments with evidence from texts"])
                }),
                new("Creative and Persuasive Writing", new()
                {
                    new("Creative Writing Techniques", ["Write descriptive narratives using sensory details", "Use dialogue effectively to advance the plot"]),
                    new("Persuasive Techniques", ["Identify and use rhetorical strategies in persuasive writing", "Write a persuasive essay with a clear claim and supporting reasons"])
                })
            }),
            new("Unit 3: Language & Grammar", new()
            {
                new("Parts of Speech", new()
                {
                    new("Nouns, Pronouns, and Adjectives", ["Identify and use common and proper nouns correctly", "Choose correct pronoun-antecedent agreement"]),
                    new("Verbs and Adverbs", ["Identify and use action, linking, and helping verbs", "Apply correct verb tense in writing"])
                }),
                new("Sentence Structure", new()
                {
                    new("Types of Sentences", ["Identify simple, compound, complex, and compound-complex sentences", "Combine short sentences to improve writing fluency"]),
                    new("Punctuation and Mechanics", ["Use commas, semicolons, and colons correctly", "Apply rules of capitalization and end punctuation"])
                }),
                new("Vocabulary Development", new()
                {
                    new("Word Study Strategies", ["Use context clues to determine word meaning", "Identify prefixes, suffixes, and root words to expand vocabulary"]),
                    new("Academic Vocabulary", ["Learn and apply subject-specific vocabulary across content areas", "Use a thesaurus and dictionary effectively"])
                })
            })
        };

        private static List<UnitSeed> ArabicCurriculum() => new()
        {
            new("الوحدة الأولى: القراءة والفهم", new()
            {
                new("النصوص الأدبية", new()
                {
                    new("القصة القصيرة", ["تحليل عناصر القصة: الشخصية والحدث والمكان والزمان", "استنتاج الفكرة الرئيسية والأفكار الجزئية من النص"]),
                    new("الفهم القرائي", ["توظيف مهارات القراءة الناقدة والاستيعابية", "التمييز بين الحقيقة والرأي في النصوص"])
                }),
                new("الشعر والأدب", new()
                {
                    new("القصيدة العربية", ["التعرف على خصائص الشعر العربي وأوزانه", "تحليل الصور البيانية والمحسنات البديعية"]),
                    new("الأدب الحديث", ["قراءة نماذج من الأدب العربي الحديث وتحليلها", "المقارنة بين الأدب القديم والحديث"])
                }),
                new("النصوص المعلوماتية", new()
                {
                    new("المقالة والتقرير", ["تحديد هدف الكاتب في النص المعلوماتي", "استخراج المعلومات الرئيسية وتنظيمها"])
                })
            }),
            new("الوحدة الثانية: القواعد النحوية والصرفية", new()
            {
                new("النحو", new()
                {
                    new("المبتدأ والخبر", ["تحديد المبتدأ والخبر في الجملة الاسمية", "توظيف الجملة الاسمية في الكتابة"]),
                    new("الفاعل والمفعول به", ["تحديد الفاعل والمفعول به في الجملة الفعلية", "ضبط الأسماء بالحركات الإعرابية الصحيحة"])
                }),
                new("الصرف", new()
                {
                    new("الأفعال وأوزانها", ["استخراج مصادر الأفعال وتصريفها", "التمييز بين الفعل المجرد والمزيد"]),
                    new("المشتقات", ["تكوين اسم الفاعل واسم المفعول", "توظيف المشتقات في الجمل"])
                }),
                new("الإملاء والكتابة", new()
                {
                    new("قواعد الإملاء", ["كتابة الهمزات في مواضعها الصحيحة", "التمييز بين التاء المفتوحة والمربوطة"])
                })
            }),
            new("الوحدة الثالثة: التعبير والكتابة", new()
            {
                new("التعبير الكتابي", new()
                {
                    new("الفقرة المتماسكة", ["كتابة فقرة متماسكة ذات فكرة رئيسية وأفكار داعمة", "توظيف أدوات الربط لتحسين التماسك النصي"]),
                    new("المقالة", ["كتابة مقالة وصفية وأخرى رأيية", "مراجعة الكتابة وتحريرها"])
                }),
                new("المحادثة والتعبير الشفهي", new()
                {
                    new("مهارات التحدث", ["إلقاء خطاب أمام الزملاء بطلاقة", "المشاركة الفعالة في النقاشات الصفية"])
                })
            })
        };

        private static List<UnitSeed> SocialStudiesCurriculum(string subjectName) => new()
        {
            new("Unit 1: Geography and the Physical World", new()
            {
                new("Maps and Geographic Tools", new()
                {
                    new("Reading Maps and Globes", ["Identify and use map keys, scales, and compass roses", "Distinguish between physical and political maps"]),
                    new("Geographic Features", ["Describe major landforms and bodies of water", "Explain how physical geography influences human settlement"])
                }),
                new("Climate and Ecosystems", new()
                {
                    new("Climate Zones", ["Identify the world's major climate zones and their characteristics", "Explain the factors that affect climate"]),
                    new("Biomes and Human Impact", ["Describe the major biomes and their biodiversity", "Analyze the impact of human activities on natural environments"])
                })
            }),
            new("Unit 2: History and Civilization", new()
            {
                new("Ancient Civilizations", new()
                {
                    new("Early Human Societies", ["Describe the transition from hunter-gatherer to agricultural societies", "Identify key features of early river civilizations"]),
                    new("Ancient Empires", ["Analyze the rise and fall of ancient empires", "Evaluate the lasting contributions of ancient civilizations"])
                }),
                new("Modern History", new()
                {
                    new("Political Developments", ["Trace the development of democratic systems of government", "Analyze causes and effects of major historical events"]),
                    new("Economic History", ["Explain the development of trade routes and economic systems", "Describe the impact of industrialization on society"])
                })
            }),
            new("Unit 3: Civics and Society", new()
            {
                new("Government and Citizenship", new()
                {
                    new("Systems of Government", ["Compare different systems of government", "Explain the rights and responsibilities of citizens"]),
                    new("Rule of Law", ["Describe the purpose of laws in a society", "Analyze how laws protect individual rights and freedoms"])
                }),
                new("Economic Systems", new()
                {
                    new("Supply and Demand", ["Explain the principles of supply and demand", "Analyze how markets determine prices"]),
                    new("Global Economy", ["Describe the role of trade in the global economy", "Evaluate the impact of globalization on local communities"])
                })
            })
        };

        private static List<UnitSeed> ComputingCurriculum() => new()
        {
            new("Unit 1: Fundamentals of Computing", new()
            {
                new("Computer Systems", new()
                {
                    new("Hardware and Software", ["Identify and describe the function of key hardware components", "Distinguish between system software and application software"]),
                    new("Operating Systems", ["Explain the role of an operating system", "Perform basic file management tasks"])
                }),
                new("Data Representation", new()
                {
                    new("Binary and Number Systems", ["Convert numbers between binary, decimal, and hexadecimal", "Represent text and images in binary"]),
                    new("Data Storage and Compression", ["Explain units of data storage (bits, bytes, kilobytes)", "Describe lossless and lossy compression methods"])
                })
            }),
            new("Unit 2: Programming and Problem Solving", new()
            {
                new("Algorithmic Thinking", new()
                {
                    new("Flowcharts and Pseudocode", ["Design algorithms using flowcharts and pseudocode", "Trace through an algorithm to predict its output"]),
                    new("Decomposition and Abstraction", ["Break down complex problems into smaller sub-problems", "Identify patterns and generalizations in problems"])
                }),
                new("Introduction to Programming", new()
                {
                    new("Variables and Data Types", ["Declare and use variables of different data types", "Perform arithmetic and comparison operations"]),
                    new("Control Structures", ["Implement selection (if/else) and repetition (loops) in code", "Debug simple programs and correct logical errors"])
                })
            }),
            new("Unit 3: Networks and Cybersecurity", new()
            {
                new("Computer Networks", new()
                {
                    new("Network Types and Topology", ["Describe LAN, WAN, and the internet", "Explain how devices communicate using protocols"]),
                    new("The Internet and World Wide Web", ["Explain how the internet works using IP addresses and DNS", "Describe the difference between the internet and the World Wide Web"])
                }),
                new("Cybersecurity and Digital Citizenship", new()
                {
                    new("Online Safety", ["Identify common cybersecurity threats and how to mitigate them", "Apply safe practices for passwords and personal data"]),
                    new("Digital Ethics", ["Explain copyright and intellectual property in a digital context", "Describe responsible use of social media and online communication"])
                })
            })
        };

        private static List<UnitSeed> IslamicStudiesCurriculum() => new()
        {
            new("الوحدة الأولى: العقيدة الإسلامية", new()
            {
                new("أركان الإيمان", new()
                {
                    new("الإيمان بالله", ["شرح معنى التوحيد وأنواعه", "استدلال على وجود الله من آيات الكون"]),
                    new("الإيمان بالملائكة والكتب والرسل", ["التعرف على أركان الإيمان الستة", "بيان الحكمة من الإيمان بالملائكة والكتب"])
                }),
                new("الإيمان باليوم الآخر", new()
                {
                    new("علامات الساعة", ["التمييز بين علامات الساعة الكبرى والصغرى", "بيان أثر الإيمان باليوم الآخر على السلوك"]),
                    new("الجنة والنار", ["وصف ما أعد الله للمؤمنين في الجنة", "استخراج الدروس والعبر من أحوال الآخرة"])
                })
            }),
            new("الوحدة الثانية: الفقه والعبادات", new()
            {
                new("الصلاة", new()
                {
                    new("فرائض الصلاة وسننها", ["ذكر فرائض الصلاة وشروطها", "أداء الصلاة بشكل صحيح وفق السنة النبوية"]),
                    new("صلاة الجماعة", ["بيان فضل صلاة الجماعة وأحكامها", "التعرف على أحكام الإمامة والاقتداء"])
                }),
                new("الزكاة والصيام", new()
                {
                    new("فريضة الزكاة", ["تعريف الزكاة وحكمة تشريعها", "بيان مصارف الزكاة الثمانية"]),
                    new("فريضة الصيام", ["شرح أركان الصوم وما يبطله", "استخراج الحكم والفوائد من فريضة الصوم"])
                })
            }),
            new("الوحدة الثالثة: السيرة النبوية والأخلاق", new()
            {
                new("السيرة النبوية", new()
                {
                    new("حياة النبي ﷺ", ["استعراض أهم محطات سيرة النبي ﷺ", "استخراج الدروس والعبر من حياة النبي ﷺ"]),
                    new("غزوات النبي ﷺ", ["التعرف على أسباب ونتائج الغزوات الكبرى", "بيان الدروس المستفادة من غزوة بدر وأحد"])
                }),
                new("الأخلاق الإسلامية", new()
                {
                    new("الصدق والأمانة", ["تعريف الصدق وفضله في الإسلام", "بيان أثر الأمانة في بناء المجتمع"]),
                    new("التسامح والتعاون", ["التعرف على مفهوم التسامح في الإسلام", "ذكر أمثلة على التعاون والتكافل الاجتماعي"])
                })
            })
        };

        private static List<UnitSeed> GenericCurriculum(string subjectName) => new()
        {
            new($"Unit 1: Introduction to {subjectName}", new()
            {
                new("Foundations and Basic Concepts", new()
                {
                    new("Core Terminology", [$"Define key terms and concepts in {subjectName}", $"Explain the scope and importance of {subjectName}"]),
                    new("Historical Development", [$"Trace the historical development of {subjectName}", "Identify key contributors and milestones in the field"])
                }),
                new("Fundamental Principles", new()
                {
                    new("Core Principles", [$"Identify and explain the fundamental principles of {subjectName}", "Apply basic principles to solve introductory problems"]),
                    new("Methods and Approaches", [$"Describe standard methods used in {subjectName}", "Choose appropriate approaches for different types of problems"])
                })
            }),
            new($"Unit 2: Core Concepts in {subjectName}", new()
            {
                new("Main Themes and Topics", new()
                {
                    new("Key Concepts", [$"Analyze the major themes and topics in {subjectName}", "Connect concepts to real-world applications"]),
                    new("Relationships and Patterns", [$"Identify patterns and relationships within {subjectName}", "Use critical thinking to evaluate concepts"])
                }),
                new("Application and Practice", new()
                {
                    new("Practical Applications", [$"Apply knowledge of {subjectName} to solve problems", "Demonstrate understanding through practical exercises"]),
                    new("Case Studies", [$"Analyze case studies related to {subjectName}", "Draw conclusions and make recommendations"])
                })
            }),
            new($"Unit 3: Advanced Topics in {subjectName}", new()
            {
                new("Advanced Concepts", new()
                {
                    new("In-Depth Analysis", [$"Conduct in-depth analysis of advanced topics in {subjectName}", "Synthesize information from multiple sources"]),
                    new("Research and Inquiry", [$"Design and conduct inquiry-based investigations in {subjectName}", "Communicate findings clearly and effectively"])
                }),
                new("Evaluation and Assessment", new()
                {
                    new("Critical Evaluation", [$"Critically evaluate theories and concepts in {subjectName}", "Assess the strengths and limitations of different approaches"]),
                    new("Synthesis and Integration", [$"Integrate knowledge from different areas of {subjectName}", "Demonstrate mastery through comprehensive projects"])
                })
            })
        };

        private record UnitSeed(string Name, List<LessonSeed> Lessons);
        private record LessonSeed(string Name, List<TopicSeed> Topics);
        private record TopicSeed(string Name, List<string> LearningOutcomes);
    }
}
