// NAnt - A .NET build tool
// Copyright (C) 2001-2002 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Jaroslaw Kowalski (jkowalski@users.sourceforge.net)

using System;
using System.IO;
using System.Globalization;

using NUnit.Framework;
using System.Collections;
using System.Collections.Specialized;

using NAnt.Core;
using Tests.NAnt.Core.Util;

namespace Tests.NAnt.Core {
    [TestFixture] public class ExpressionEvaluatorTest : BuildTestBase {
        #region Private Instance Fields

        private string _format = @"<?xml version='1.0'?>
            <project name='ProjectTest' default='test' basedir='{0}'>
                {1}
                <target name='test'>
                    {2}
                </target>
            </project>";

        private string _buildFileName;
        private Project _project;

        #endregion Private Instance Fields

        [SetUp] protected override void SetUp() {
            base.SetUp();
            _buildFileName = Path.Combine(TempDirName, "test.build");
            TempFile.CreateWithContents(FormatBuildFile("", ""), _buildFileName);

            //_project = new Project(_buildFileName, Level.Debug);
            _project = new Project(_buildFileName, Level.Info);
            _project.Properties["prop1"] = "asdf";
        }

        [TearDown] protected override void TearDown() {
        }
        #region Public Instance Methods
        
        [Test] public void TestCoreOperations() {
            AssertExpression("1+2", 3);
            AssertExpression("1+2+3", 6);
            AssertExpression("1+2*3", 7);
            AssertExpression("2*1*3", 6);
            AssertExpression("1/2+3", 3);
            AssertExpression("5.0/(2+8)", 0.5);
            AssertExpression("((((1))))", 1);
            AssertExpression("((((1+2))))", 3);
            AssertExpression("((((1+2)+(2+1))))", 6);
            AssertExpression("((((1+2)/(2+1))))", 1);
            AssertExpression("-1", -1);
            AssertExpression("--1", 1);
            AssertExpression("10 % 3", 1);
            AssertExpression("10 % 3 % 5", 1);
            AssertExpression("-1 = 1-2", true);
            AssertExpression("--1.0 = 1.0", true);
            AssertExpression("1 <> 1", false);
            AssertExpression("1 = 2", false);
            AssertExpression("10.0 - 1.0 >= 8.9", true);
            AssertExpression("10.0 + 1 <= 11.1", true);
            AssertExpression("1 * 1.0 = 1.0", true);
            AssertFailure("1.aaaa"); // fractional part expected
            AssertFailure("(1 1)");
            AssertFailure("aaaa::1");
            AssertFailure("aaaa::bbbb 1");
        }
        
        [Test] public void TestCoreOperationFailures() {
            AssertFailure("1+aaaa");
            AssertFailure("1+");
            AssertFailure("*3");
            AssertFailure("2*/1*3");
            AssertFailure("1//2+3");
            AssertFailure("convert::todouble(5)/(2+8)");
            AssertFailure("convert::to-double(1/2+3");
            AssertFailure("-'aaa'");
            AssertFailure("true + true");
            AssertFailure("true - true");
            AssertFailure("true * true");
            AssertFailure("true / true");
            AssertFailure("true % true");
            AssertFailure("((((1)))");
            AssertFailure("((1+2))))");
            AssertFailure("((((1+2)+(2+1)))");
            AssertFailure("5/0");
            AssertFailure("5%0");
            AssertFailure("convert::to-double(5)/(2+8)");
        }
        
        [Test] public void TestRelationalOperators() {
            AssertExpression("'a' = 'a'", true);
            AssertExpression("'a' = 'b'", false);
            AssertExpression("'a' <> 'a'", false);
            AssertExpression("'a' <> 'b'", true);
            AssertExpression("'a' + 'b' = 'ab'", true);
            AssertExpression("1 = 1", true);
            AssertExpression("1 < 2", true);
            AssertExpression("1 > 2", false);
            AssertExpression("2 < 1", false);
            AssertExpression("2 > 1", true);
            AssertExpression("2 <= 1", false);
            AssertExpression("2 >= 1", true);
            AssertExpression("1 <> 2", true);
            AssertExpression("1.0 = 1.0", true);
            AssertExpression("1.0 <> 1.0", false);
            AssertExpression("1.0 = 2.0", false);
            AssertExpression("1.0 <> 2.0", true);
            AssertExpression("true", true);
            AssertExpression("false", false);
            AssertExpression("true==true", true);
            AssertExpression("true==false", false);
            AssertExpression("true<>false", true);
            AssertExpression("true<>true", false);
        }
        
        [Test] public void TestLogicalOperators() {
            AssertExpression("true or false or false", true);
            AssertExpression("false or false or false", false);
            AssertExpression("false or true", true);
            AssertExpression("true and false", false);
            AssertExpression("true and true and false", false);
            AssertExpression("true and true and true", true);
            AssertExpression("false and true and true", false);
            AssertExpression("not true", false);
            AssertExpression("not false", true);
            AssertExpression("not (1=1)", false);
            AssertExpression("true or not (1=1)", true);
            AssertExpression("true or not (--1=1)", true);
        }
        
        [Test] public void TestConversionFunctions() {
            AssertExpression("convert::to-double(5)/(2+8)", 0.5);
            AssertExpression("convert::to-double(1)/2+3", 3.5);
            AssertExpression("convert::to-datetime('12/31/1999 01:23:34')", new DateTime(1999,12,31,1,23,34));
            AssertExpression("convert::to-datetime(convert::to-datetime('12/31/1999 01:23:34'))", new DateTime(1999,12,31,1,23,34));
            AssertFailure("convert::to-int(datetime::now())");
            AssertFailure("convert::to-double('aaaaaaaaa')");
            AssertFailure("convert::to-datetime(1)");
            AssertFailure("convert::to-boolean(1)");
            AssertExpression("convert::to-boolean('True')",true);
            AssertExpression("convert::to-boolean('true')",true);
            AssertExpression("convert::to-boolean('False')",false);
            AssertExpression("convert::to-boolean('false')",false);
            AssertFailure("convert::to-boolean('aaafalse')");
            AssertExpression("convert::to-string(false)","False");
            AssertExpression("convert::to-string(1)","1");
            AssertExpression("convert::to-int('123'+'45')",12345);
        }
        [Test] public void TestStringFunctions() {
            AssertExpression("string::get-length('')", 0);
            AssertExpression("string::get-length('')=0", true);
            AssertExpression("string::get-length('')=1", false);
            AssertExpression("string::get-length('test')", 4);
            AssertExpression("string::get-length('test')=4", true);
            AssertExpression("string::get-length('test')=5", false);
            AssertExpression("string::get-length(prop1)", 4);
            AssertExpression("string::get-length('d''Artagnan')", 10);
            AssertExpression("string::get-length('d''Artagnan')=10", true);
            AssertExpression("string::get-length('d''Artagnan')=11", false);
            AssertExpression("string::substring('abcde',1,2)='bc'", true);
            AssertExpression("string::trim('  ab  ')='ab'", true);
            AssertExpression("string::trim-start('  ab  ')='ab  '", true);
            AssertExpression("string::trim-end('  ab  ')='  ab'", true);
            AssertExpression("string::pad-left('ab',5,'.')='...ab'", true);
            AssertExpression("string::pad-right('ab',5,'.')='ab...'", true);
            AssertExpression("string::index-of('abc','c')=2", true);
            AssertExpression("string::index-of('abc','d')=-1", true);
            AssertExpression("string::index-of('abc','d')=-1", true);
        }
        
        [Test] public void TestDateTimeFunctions() {
            AssertFailure("datetime::now(111)");
            AssertFailure("datetime::add()");
            AssertFailure("datetime::now(");
        }
        
        [Test] public void TestMathFunctions() {
            AssertExpression("math::round(0.1)", 0.0);
            AssertExpression("math::round(0.7)", 1.0);
            AssertExpression("math::floor(0.1)", 0.0);
            AssertExpression("math::floor(0.7)", 0.0);
            AssertExpression("math::ceiling(0.1)", 1.0);
            AssertExpression("math::ceiling(0.7)", 1.0);
            AssertExpression("math::abs(1)", 1.0);
            AssertExpression("math::abs(-1)", 1.0);
        }
        
        [Test] public void TestConditional() {
            AssertExpression("if(true,1,2)", 1);
            AssertExpression("if(true,'a','b')", "a");
            AssertExpression("if(false,'a','b')", "b");
            AssertFailure("if(1,2,3)");
            AssertFailure("if(true 2,3)");
            AssertFailure("if(true,2,3 3");
            AssertFailure("if(true,2 2,3)");
            AssertFailure("if [ true, 1, 0 ]");
        }
        
        [Test] public void TestFileFunctions() {
            AssertExpression("file::exists('c:\\i_am_not_there.txt')", false);
            AssertFailure("file::get-last-write-time('c:/no-such-file.txt')");
        }
        
        [Test] public void TestDirectoryFunctions() {
            AssertExpression("directory::exists('c:\\i_am_not_there')", false);
            AssertExpression("directory::exists('" + Directory.GetCurrentDirectory() + "')", true);
        }
        
        [Test] public void TestNAntFunctions() {
            AssertExpression("property::get-value('prop1')", "asdf");
            AssertExpression("property::exists('prop1')", true);
            AssertExpression("property::exists('prop1a')", false);
            //AssertExpression("target::exists('i_am_not_there')", false);
            //AssertExpression("target::exists('test')", true);
        }

        [Test] public void TestStandaloneEvaluator() {
            ExpressionEvaluator eval = 
                new ExpressionEvaluator(_project, 
                        _project.Properties, 
                        Location.UnknownLocation, 
                        new Hashtable(), 
                        new Stack());
            
            Assert.AreEqual(eval.Evaluate("1+2*3"), 7);
            eval.CheckSyntax("1+2*3");
        }
        
        [Test]
        [ExpectedException(typeof(ExpressionParseException))]
        public void TestStandaloneEvaluatorFailure() {
            ExpressionEvaluator eval = new ExpressionEvaluator(_project, 
                    _project.Properties, 
                    Location.UnknownLocation, 
                    new Hashtable(), 
                    new Stack());

            eval.Evaluate("1+2*datetime::now(");
        }
        
        [Test]
        [ExpectedException(typeof(ExpressionParseException))]
        public void TestStandaloneEvaluatorFailure2() {
            ExpressionEvaluator eval = new ExpressionEvaluator(_project, 
                    _project.Properties, 
                    Location.UnknownLocation, 
                    new Hashtable(), 
                    new Stack());

            eval.Evaluate("1 1");
        }
        
        [Test]
        [ExpectedException(typeof(ExpressionParseException))]
        public void TestStandaloneEvaluatorSyntaxCheckFailure() {
            ExpressionEvaluator eval = new ExpressionEvaluator(_project, 
                    _project.Properties, 
                    Location.UnknownLocation, 
                    new Hashtable(), 
                    new Stack());

            eval.CheckSyntax("1+2*3 1");
        }
        
        #endregion
        
        #region Private Instance Methods

        private void AssertExpression(string expression, object expectedReturnValue) {
            string value = _project.ExpandProperties("${" + expression + "}", Location.UnknownLocation);
            string expectedStringValue = Convert.ToString(expectedReturnValue, CultureInfo.InvariantCulture);

            _project.Log(Level.Debug, "expression: " + expression);
            _project.Log(Level.Debug, "value: " + value + ", expected: " + expectedStringValue);
            Assert.AreEqual(expectedStringValue, value, expression);
        }

        private void AssertFailure(string expression) {
            try {
                string value = _project.ExpandProperties("${" + expression + "}", Location.UnknownLocation);
                // we shouldn't get here
                Assert.Fail("Expected BuildException while evaluating ${" + expression + "}, nothing was thrown. The returned value was " + value);
            } catch (BuildException ex) {
                _project.Log(Level.Debug, "Got expected failure on ${" + expression + "}: " + ((ex.InnerException != null) ? ex.InnerException.Message : ""));
                // ok - this one should have been thrown
            } catch (Exception ex) {
                // some other exception has been thrown - fail
                Assert.Fail("Expected BuildException while evaluating ${" + expression + "}, but " + ex.GetType().FullName + " was thrown.");
            }
        }

        private string FormatBuildFile(string globalTasks, string targetTasks) {
            return string.Format(CultureInfo.InvariantCulture, _format, TempDirName, globalTasks, targetTasks);
        }

        #endregion

    }
}

