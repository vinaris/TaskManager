using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using Microsoft.EntityFrameworkCore;
using Task = TaskManager.Models.Task;
using TaskStatus = TaskManager.Models.TaskStatus;

namespace TaskManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly TaskManagerDbContext _context;
        private bool _flag;

        public HomeController(TaskManagerDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? selectedTaskId, string message = "")
        {
            TasksList taskList = new TasksList
            {
                Tasks = await _context.Tasks.OrderByDescending(t => t.DateOfRegistration).ToListAsync()
            };
            ViewBag.SelectedTask = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == selectedTaskId);
            if (message != "")
            {
                ViewBag.Message = message;
            }
            return View(taskList);
        }
        [HttpGet]
        public async Task<IActionResult> Create(int? parentId)
        {
            var newTask = new Task
            {
                EndDate = DateTime.Today
            };
            if (parentId != null)
            {
                newTask.ParentTaskId = parentId;
                var parentTask = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);
                if (parentTask.TaskStatus==TaskStatus.Completed)
                {
                    return RedirectToAction("Index", new { selectedTaskId = parentTask.Id, message = "Для завершённых задач нельзя добавлять новые подзачи." });
                }
                newTask.ListOfPerformers = parentTask.ListOfPerformers;
            }
            return View(newTask);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Task task)
        {
            if (ModelState.IsValid)
            {
                task.Id = 0;
                task.DateOfRegistration = DateTime.Now;
                task.TaskStatus = TaskStatus.Appointed;
                _context.Add(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { selectedTaskId = task.Id });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            return View(await _context.Tasks.SingleOrDefaultAsync(t => t.Id == id));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            DeleteNodes((int)id);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private void DeleteNodes(int id)
        {
            var inner = _context.Tasks.Where(x => x.ParentTaskId == id).ToList();
            foreach (var node in inner)
            {
                DeleteNodes(node.Id);
            }
            var deleted = _context.Tasks.SingleOrDefault(x => x.Id == id);
            if (deleted != null)
            {
                _context.Tasks.Remove(deleted);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            return View(await _context.Tasks.SingleOrDefaultAsync(t => t.Id == id));
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Task task)
        {
            if (ModelState.IsValid)
            {
                _context.Update(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { selectedTaskId = task.Id});
        }

        
        public async Task<IActionResult> Play(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == id);
            task.TaskStatus = TaskStatus.Performed;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { selectedTaskId = id });
        }

        public async Task<IActionResult> Stop(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == id);
            task.TaskStatus = TaskStatus.Suspended;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { selectedTaskId = id });
        }

        public async Task<IActionResult> Perform(int? id, double count)
        {
            if (id == null)
            {
                return NotFound();
            }
            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == id);
            var str = "";
            if (task.TaskStatus != TaskStatus.Appointed)
            {
                _flag = false;
                FindCompleteChild(id);
                if (!_flag)
                {
                    task.EndDate = DateTime.Now;
                    task.PerformTime = count;
                    _context.Tasks.Update(task);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    str = "Задача не может быть выполненной, поскольку одна из подзадач имеет статус \"Назначена\".";
                }
            }
            else
            {
                str = "Выбранная задача должна иметь статус \"Выполняется\" или \"Приостановлена\"";
            }
            return RedirectToAction("Index", new { selectedTaskId = id, message = str });
        }

        private void FindCompleteChild(int? id)
        {
            var inner = _context.Tasks.Where(x => x.ParentTaskId == id).ToList();
            foreach (var node in inner)
            {
                FindCompleteChild(node.Id);
            }
            var completed = _context.Tasks.SingleOrDefault(x => x.Id == id);
            if (completed != null)
            {
                if (completed.TaskStatus == TaskStatus.Appointed)
                {
                    _flag = true;
                    return;
                }
                completed.TaskStatus = TaskStatus.Completed;
                if (completed.PerformTime == null || completed.PerformTime == 0)
                {
                    completed.PerformTime = completed.Laboriousness;
                }
                if (completed.EndDate == new DateTime())
                {
                    completed.EndDate = DateTime.Now;
                }
            }
           
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
