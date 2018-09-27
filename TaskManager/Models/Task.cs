using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Models
{
    public enum TaskStatus
    {
        [Display(Name = "Назначена")]
        Appointed,
        [Display(Name = "Выполняется")]
        Performed,
        [Display(Name = "Приостановлена")]
        Suspended,
        [Display(Name = "Завершена")]
        Completed
    }

    public class Task
    {
        [Display(Name = "Номер")]
        public int Id { get; set; }
        [Display(Name = "Наименование")]
        [Required]
        public string Name { get; set; }
        [Display(Name = "Описание")]
        [Required]
        public string Description { get; set; }
        [Display(Name = "Список исполнителей")]
        [Required]
        public string ListOfPerformers { get; set; }
        [Display(Name = "Дата регистрации")]
        public DateTime DateOfRegistration { get; set; }
        [Display(Name = "Статус")]
        public TaskStatus TaskStatus { get; set; }
        [Display(Name = "Плановая трудоёмкость задачи")]
        public double Laboriousness { get; set; }
        [Display(Name = "Дата завершения")]
        public DateTime EndDate { get; set; }
        [Display(Name = "Фактическое время выполннения")]
        public double? PerformTime { get; set; }
        public int? ParentTaskId { get; set; }
    }
}
