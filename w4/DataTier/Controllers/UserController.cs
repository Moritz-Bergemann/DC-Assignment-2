using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APIClasses;
using DataTier.Models;

namespace DataTier.Controllers
{
    public class UserController : ApiController
    {

        [Route("api/User/{userId}")]
        [HttpGet]
        public UserData GetUser(uint userId)
        {
            UserData data = new UserData();

            data.id = userId;
            DataModel.Instance.GetUserName(userId, out data.fName, out data.lName);

            return data;
        }

        [Route("api/User")]
        [HttpGet]
        public List<UserData> GetAllUsers()
        {
            List<uint> userIds = DataModel.Instance.GetUserIds();
            List<UserData> allUsers = new List<UserData>();

            foreach (uint userId in userIds)
            {
                UserData data = new UserData();

                data.id = userId;
                DataModel.Instance.GetUserName(userId, out data.fName, out data.lName);
                
                allUsers.Add(data);
            }

            return allUsers;
        }

        [Route("api/User")]
        [HttpPost]
        public uint CreateUser(CreateUserData createData)
        {
            return DataModel.Instance.CreateUser(createData.fName, createData.lName);
        }
    }
}