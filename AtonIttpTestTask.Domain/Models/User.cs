

namespace AtonIttpTestTask.Domain.Models
{
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя
        /// </summary>
        public Guid Guid { get; set; }
        /// <summary>
        /// Уникальный Логин (запрещены все символы кроме латинских букв и цифр)
        /// </summary>
        public string Login { get; set; }
        /// <summary>
        /// Пароль (запрещены все символы кроме латинских букв и цифр)
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Имя (запрещены все символы кроме латинских и русских букв)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Пол 0 - женщина, 1 - мужчина, 2 - неизвестно
        /// </summary>
        public int Gender { get; set; } = 2;
        /// <summary>
        /// Дата рождения, может быть Null
        /// </summary>
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// Указание - является ли пользователь админом
        /// </summary>
        public bool Admin { get; set; }
        /// <summary>
        /// Дата создания пользователя
        /// </summary>
        public DateTime CreatedOn { get; set; }
        /// <summary>
        /// Логин Пользователя, от имени которого этот пользователь создан
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// Дата изменения пользователя
        /// </summary>
        public DateTime ModifiedOn { get; set; }
        /// <summary>
        /// Логин Пользователя, от имени которого этот пользователь изменён
        /// </summary>
        public string ModifiedBy { get; set; }
        /// <summary>
        /// Дата удаления пользователя
        /// </summary>
        public DateTime? RevokedOn { get; set; }
        /// <summary>
        /// Логин Пользователя, от имени которого этот пользователь удалён
        /// </summary>
        public string? RevokedBy { get; set; }
    }
}
