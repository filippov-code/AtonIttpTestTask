using AtonIttpTestTask.Repository.DataBases;
using AtonIttpTestTask.Domain.Interfaces;
using AtonIttpTestTask.Domain.DTO;
using AtonIttpTestTask.Domain.Models;
using AtonIttpTestTask.Repository.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace AtonIttpTestTask.Controllers
{
    [Route("api/[controller]/[action]")]
    //ЗАТЕСТИТЬ МЕТОД СТАРШЕ ЧЕМ
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        #region CREATE
        /// <summary>
        /// 1) Создание пользователя по логину, паролю, имени, полу и дате рождения + указание будет ли пользователь админом(Доступно Админам)
        /// </summary>
        /// <param name="callerLogin"></param>
        /// <param name="callerPassword"></param>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="name"></param>
        /// <param name="gender"></param>
        /// <param name="birthday"></param>
        /// <param name="isAdmin"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(
            string callerLogin, string callerPassword, 
            string login, string password,
            string name, int gender, DateTime? birthday, bool isAdmin
        )
        {
            if (!IsStringValid(login) || !IsStringValid(password) || !IsStringValid(name))
                return StringIsNotValid("Login, password or name");
            if (!IsGenderValid(gender))
                return GenderIsNotValid();
            if (birthday != null && birthday > DateTime.Now)
                return BirthdayIsNotValid();
            if (!await _userRepository.IsLoginAreAvailableAsync(login))
                return LoginAreBusy();

            User? caller = await _userRepository.GetByLoginAndPasswordAsync(callerLogin, callerPassword);
            if (caller == null)
                return CallerNotFound();
            if (!caller.Admin)
                return Forbidden();

            User newUser = new User
            {
                Login = login,
                Password = password,
                Name = name,
                Gender = gender,
                Birthday = birthday,
                Admin = isAdmin,
                CreatedOn = DateTime.Now,
                CreatedBy = caller.Name,
                ModifiedOn = DateTime.Now,
                ModifiedBy = caller.Name
            };

            await _userRepository.CreateAsync(newUser);
            await _userRepository.SaveAsync();
            return CreatedAtAction(nameof(GetUser), value: newUser );
        }
        #endregion
        
        #region READ
        /// <summary>
        /// 5) Запрос списка всех активных (отсутствует RevokedOn) пользователей, список отсортирован по CreatedOn(Доступно Админам)
        /// </summary>
        /// <param name="callerLogin"></param>
        /// <param name="callerPassword"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllActive(string callerLogin, string callerPassword)
        {
            User? caller = await _userRepository.GetByLoginAndPasswordAsync(callerLogin, callerPassword);
            if (caller == null)
                return CallerNotFound();
            if (!caller.Admin)
                return Forbidden();

            var result = await _userRepository.GetAllActiveInDescendingOrderOfCreatedOnAsync();

            return Ok(result);
        }
        /// <summary>
        /// 6) Запрос пользователя по логину, в списке долны быть имя, пол и дата рождения статус активный или нет(Доступно Админам)
        /// </summary>
        /// <param name="callerLogin">Логин вызывающего</param>
        /// <param name="callerPassword"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<UserDTO>> GetUserPreview(string callerLogin, string callerPassword, string login)
        {
            var caller = await _userRepository.GetByLoginAndPasswordAsync(callerLogin, callerPassword);
            if (caller == null)
                return CallerNotFound();
            if (!caller.Admin)
                return Forbidden();
            var user = await _userRepository.GetByLoginAsync(login);
            if (user == null)
                return UserNotFound();

            var userDto = new UserDTO(user);

            return userDto;

        }
        /// <summary>
        /// 7) Запрос пользователя по логину и паролю (Доступно только самому пользователю, если он активен(отсутствует RevokedOn))
        /// </summary>
        /// <param name="callerLogin"></param>
        /// <param name="callerPassword"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<User>> GetUser(string callerLogin, string callerPassword)
        {
            var caller = await _userRepository.GetByLoginAndPasswordAsync(callerLogin, callerPassword);
            if (caller == null)
                return CallerNotFound();
            if (caller.RevokedOn != null)
                return Forbidden();

            return caller;
        }
        /// <summary>
        /// 8) Запрос всех пользователей старше определённого возраста (Доступно Админам)
        /// </summary>
        /// <param name="callerLogin"></param>
        /// <param name="callerPassword"></param>
        /// <param name="age"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersOlderThan(string callerLogin, string callerPassword, int age)
        {
            var caller = await _userRepository.GetByLoginAndPasswordAsync(callerLogin, callerPassword);
            if (caller == null)
                return CallerNotFound();
            if (!caller.Admin)
                return Forbidden();

            var result = await _userRepository.GetOlderThanAsync(age);

            return Ok(result);
        }
        #endregion
        
        #region UPDATE
        /// <summary>
        /// 2) Изменение имени, пола или даты рождения пользователя (Может менять Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn))
        /// </summary>
        /// <param name="callerLogin"></param>
        /// <param name="callerPassword"></param>
        /// <param name="userLogin"></param>
        /// <param name="newName"></param>
        /// <param name="newGender"></param>
        /// <param name="newBirthdate"></param>
        /// <returns></returns>
        [HttpPatch]
        public async Task<IActionResult> ChangeUserData(string callerLogin, string callerPassword, string userLogin, string? newName, int? newGender, DateTime? newBirthdate)
        {
            if (!string.IsNullOrEmpty(newName) && !IsLatinLettersOrNumbers(newName))
                return StringIsNotValid(newName);
            if (newGender != null && !IsGenderValid(newGender.Value))
                return GenderIsNotValid();
            if (newBirthdate != null && newBirthdate > DateTime.Today)
                return BirthdayIsNotValid();
            var caller = await _userRepository.GetByLoginAndPasswordAsync(callerLogin, callerPassword);
            if (caller == null)
                return CallerNotFound();
            if (!IsCallerCanChangeUserData(caller, userLogin))
                return Forbidden();
            User? user = callerLogin == userLogin ? caller : await _userRepository.GetByLoginAsync(userLogin);
            if (user == null)
                return UserNotFound();

            //(User? user, ActionResult? error) = await GetAdminOrActiveUserForEditingAsync(caller, userLogin);
            //if (user == null)
            //    return error;

            bool modified = false;
            if (newName != null)
            {
                user.Name = newName;
                modified = true;
            }
            if (newGender != null)
            {
                user.Gender = newGender.Value;
                modified = true;
            }
            if (newBirthdate != null)
            {
                user.Birthday = newBirthdate.Value;
                modified = true;
            }
            if (modified)
            {
                user.ModifiedOn = DateTime.Now;
                user.ModifiedBy = caller.Name;
                _userRepository.Update(user);
                await _userRepository.SaveAsync();
            }

            return NoContent();
        }
        /// <summary>
        /// 3) Изменение пароля (Пароль может менять либо Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn))
        /// </summary>
        /// <param name="callerLogin"></param>
        /// <param name="callerPassword"></param>
        /// <param name="userLogin"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        [HttpPatch]
        public async Task<IActionResult> ChangePassword(string callerLogin, string callerPassword, string userLogin, string newPassword)
        {
            if (!IsStringValid(newPassword))
                return StringIsNotValid(newPassword);
            var caller = await _userRepository.GetByLoginAndPasswordAsync(callerLogin, callerPassword);
            if (caller == null)
                return CallerNotFound();
            if (!IsCallerCanChangeUserData(caller, userLogin))
                return Forbidden();
            User? user = callerLogin == userLogin ? caller : await _userRepository.GetByLoginAsync(userLogin);
            if (user == null)
                return UserNotFound();

            //(User? user, ActionResult? error) = await GetAdminOrActiveUserForEditingAsync(caller, userLogin);
            //if (user == null)
            //    return error;

            user.Password = newPassword;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = caller.Name;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            return NoContent();
        }
        /// <summary>
        /// 4) Изменение логина (Логин может менять либо Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn), логин должен оставаться уникальным)
        /// </summary>
        /// <param name="callerLogin"></param>
        /// <param name="callerPassword"></param>
        /// <param name="userLogin"></param>
        /// <param name="newLogin"></param>
        /// <returns></returns>
        [HttpPatch]
        public async Task<IActionResult> ChangeLogin(string callerLogin, string callerPassword, string userLogin, string newLogin)
        {
            if (!await _userRepository.IsLoginAreAvailableAsync(newLogin))
                return LoginAreBusy();
            var caller = await _userRepository.GetByLoginAndPasswordAsync(callerLogin, callerPassword);
            if (caller == null)
                return CallerNotFound();
            if (!IsCallerCanChangeUserData(caller, userLogin))
                return Forbidden();
            User? user = callerLogin == userLogin ? caller : await _userRepository.GetByLoginAsync(userLogin);
            if (user == null)
                return UserNotFound();

            //(User? user, ActionResult? error) = await GetAdminOrActiveUserForEditingAsync(caller, userLogin);
            //if (user == null)
            //    return error;

            user.Login = newLogin;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = caller.Name;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            return Ok();
        }
        /// <summary>
        /// 10) Восстановление пользователя - Очистка полей (RevokedOn, RevokedBy) (Доступно Админам)
        /// </summary>
        /// <param name="callerLogin"></param>
        /// <param name="callerPassword"></param>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        [HttpPatch]
        public async Task<IActionResult> RestoreUser(string callerLogin, string callerPassword, string userLogin)
        {
            var caller = await _userRepository.GetByLoginAndPasswordAsync(callerLogin, callerPassword);
            if (caller == null)
                return CallerNotFound();
            if (!caller.Admin)
                return Forbidden();

            User? user = await _userRepository.GetByLoginAsync(userLogin);
            if (user == null)
                return UserNotFound();

            user.RevokedOn = null;
            user.RevokedBy = null;
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = caller.Name;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
            return Ok();
        }
        #endregion

        #region DELETE
        /// <summary>
        /// 9) Удаление пользователя по логину полное или мягкое (При мягком удалении должна происходить простановка RevokedOn и RevokedBy) (Доступно Админам)
        /// </summary>
        /// <param name="callerLogin"></param>
        /// <param name="callerPassword"></param>
        /// <param name="userLogin"></param>
        /// <param name="softDelete"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(string callerLogin, string callerPassword, string userLogin, bool softDelete)
        {
            var caller = await _userRepository.GetByLoginAndPasswordAsync(callerLogin, callerPassword);
            if (caller == null)
                return CallerNotFound();
            if (!caller.Admin)
                return Forbidden();

            User? user = await _userRepository.GetByLoginAsync(userLogin);
            if (user == null)
                return UserNotFound();

            if (softDelete)
            {
                user.RevokedOn = DateTime.Now;
                user.RevokedBy = caller.Login;
                user.ModifiedOn = DateTime.Now;
                user.ModifiedBy = caller.Name;
                _userRepository.Update(user);
                await _userRepository.SaveAsync();
                return Ok();
            }
            else
            {
                _userRepository.Delete(user);
                await _userRepository.SaveAsync();
                return NoContent();
            }
        }
        #endregion
        
        #region Helpers

        private bool IsLatinLettersOrNumbers(string input)
        {
            return Regex.IsMatch(input, @"^[a-zA-Z0-9]+$");
        }

        private bool IsStringValid(string input)
        {
            return !string.IsNullOrEmpty(input) && IsLatinLettersOrNumbers(input);
        }

        private bool IsGenderValid(int gender)
        {
            return 0 <= gender && gender <= 2;
        }
        /// <summary>
        /// Проверяет может ли вызывающий пользователь вносить изменения в данные пользователя с указанным логином:
        /// Изменения могут вносить администратор или сам пользователь, если он активен
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        private bool IsCallerCanChangeUserData(User caller, string userLogin)
        {
            return caller.Login == userLogin ? caller.RevokedOn == null : caller.Admin;
        }

        #endregion

        #region HTTP Errors
        private ActionResult StringIsNotValid(string parameterName)
        {
            return BadRequest($"{parameterName} can contain only latin letters, numbers and not be empty");
        }
        private ActionResult GenderIsNotValid()
        {
            return BadRequest("The gender is specified incorrectly. It can take values between 0 and 2");
        }
        private ActionResult BirthdayIsNotValid()
        {
            return BadRequest("The date of the birthday cannot be greater than the current date");
        }
        private ActionResult LoginAreBusy()
        {
            return BadRequest("This login is busy");
        }
        private ActionResult CallerNotFound()
        {
            return NotFound("The calling user was not found");
        }
        private ActionResult UserNotFound()
        {
            return NotFound("The calling user was not found");
        }
        private ActionResult Forbidden()
        {
            return Problem(
                type: "forbidden",
                title: "You don't have the rights to do this",
                statusCode: StatusCodes.Status403Forbidden,
                instance: HttpContext.Request.Path
            );
        }
        #endregion
    }
}
