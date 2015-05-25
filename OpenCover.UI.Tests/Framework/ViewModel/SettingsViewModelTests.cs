using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using OpenCover.UI.Framework.ViewModel;

namespace OpenCoverUI.Tests.Framework.ViewModel
{
    [TestFixture]
    public class SettingsViewModelTests
    {
        private IOpenCoverUiSettings _openCoverUiSettings;
        private SettingsViewModel _model;

        [SetUp]
        public void SetUp()
        {
            _openCoverUiSettings = Substitute.For<IOpenCoverUiSettings>();
            _model = new SettingsViewModel(_openCoverUiSettings);
        }


        [Test]
        public void WhenIExecute_ProcessSelectNUnitExe_ItFiresThe_SelectNunitExeEvent()
        {
            // arrange
            var fired = false;
            _model.SelectNunitExeEvent += (sender, args) => fired = true;

            // act
            _model.ProcessSelectNUnitExe.Execute(null);

            // assert
            Assert.IsTrue(fired);
        }

        [Test]
        public void WhenIExecute_ProcessSelectXUnitExe_ItFiresThe_SelectXunitExeEvent()
        {
            // arrange
            var fired = false;
            _model.SelectXUnitExeEvent += (sender, args) => fired = true;

            // act
            _model.ProcessSelectXUnitExe.Execute(null);

            // assert
            Assert.IsTrue(fired);
        }

        [Test]
        public void WhenIExecute_ProcessSelectOpenCoverExe_ItFiresThe_SelectOpenCoverExeEvent()
        {
            // arrange
            var fired = false;
            _model.SelectOpenCoverExeEvent += (sender, args) => fired = true;

            // act
            _model.ProcessSelectOpenCoverExe.Execute(null);

            // assert
            Assert.IsTrue(fired);
        }

        [Test]
        public void Setting_NUnitExePath_UpdatesSettings()
        {
            // arrange
            Assert.AreEqual(string.Empty, _openCoverUiSettings.NUnitPath);

            // act
            _model.NUnitExePath = "1234";

            // assert
            Assert.AreEqual(_model.NUnitExePath, _openCoverUiSettings.NUnitPath);
        }

        [Test]
        public void Setting_XUnitExePath_UpdatesSettings()
        {
            // arrange
            Assert.AreEqual(string.Empty, _openCoverUiSettings.XUnitPath);

            // act
            _model.XUnitExePath = "1234";

            // assert
            Assert.AreEqual(_model.XUnitExePath, _openCoverUiSettings.XUnitPath);
        }

        [Test]
        public void Setting_OpenCoverExePath_UpdatesSettings()
        {
            // arrange
            Assert.AreEqual(string.Empty, _openCoverUiSettings.OpenCoverPath);

            // act
            _model.OpenCoverExePath = "1234";

            // assert
            Assert.AreEqual(_model.OpenCoverExePath, _openCoverUiSettings.OpenCoverPath);
        }
    }
}
