using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenkaKichi;
using SenkaKichi.Models;
using SenkaKichi.Controllers;
using System.Threading.Tasks;
using SenkaKichi.OAuthApi.Twitter;

namespace SenkaKichi.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        [TestMethod]
        public void Index() {
            // Arrange
            // Act
            // Assert
            
        }

        [TestMethod]
        public void About() {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.About() as ViewResult;

            // Assert
            Assert.AreEqual("Your application description page.", result.ViewBag.Message);
        }
    }
}
