// NAnt - A .NET build tool
// Copyright (C) 2001-2003 Gerry Shaw
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
// Clayton Harbour (claytonharbour@sporadicism.com)

using System;
using System.Globalization;
using System.IO;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Core.Util;

using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Util;

namespace NAnt.SourceControl.Tasks {
    /// <summary>
    /// Produces an xml report that represents the cvs changes from the given start day,
    /// to the given end date.  Optionally the report can be styled and output as html if 
    /// an xsl file is specified.
    /// </summary>
    /// <example>
    ///   <para>Checkout NAnt.</para>
    ///   <code>
    ///     <![CDATA[
    /// <cvschangelog   cvsroot=":pserver:anonymous@cvs.sourceforge.net:/cvsroot/nant" 
    ///                 password="" 
    ///                 module="nant"
    ///                 start="2004/07/23"
    ///                 end="2004/07/25"
    ///                 destfile="e:/test/nant/sourcecontrol/changelog-nant.xml" />
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("cvschangelog")]
    public class CvsChangeLogTask : AbstractCvsTask {

        #region Public Constants
        /// <summary>
        /// The command being executed.
        /// </summary>
        public const string CvsCommandName = "xml";

        #endregion

        #region Private Instance Fields
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Public Instance Properties

        /// <summary>
        /// The cvs command to execute.
        /// </summary>
        public override string CommandName {
            get {return CvsCommandName;}
        }

        /// <summary>
        /// Name of the xml file that will contain the cvs log information.
        /// </summary>
        [TaskAttribute("xmlfile", Required=true)]
        public string DestFile {
            get {return ((Option)CommandOptions["destfile"]).Value;}
            set {
                if (null == this.DestinationDirectory) {
                    this.DestinationDirectory = 
                        new DirectoryInfo(Path.GetDirectoryName(value));
                }
                SetCommandOption("destfile", String.Format(CultureInfo.InvariantCulture, "-oxml {0}", value), true);}
        }

        /// <summary>
        /// The earliest change to use in the cvs log command.
        /// </summary>
        [TaskAttribute("start", Required=true)]
        [DateTimeValidator()]
        public DateTime StartDate {
            get {return Convert.ToDateTime(((Option)CommandOptions["start"]).Value);}
            set {SetCommandOption("start", String.Format(CultureInfo.InvariantCulture,"-D \"{0}\"", DateParser.GetCvsDateString(value)), true);}
        }

        /// <summary>
        /// The latest date to use in the cvs log command.
        /// </summary>
        [TaskAttribute("end", Required=true)]
        [DateTimeValidator()]
        public DateTime EndDate {
            get {return Convert.ToDateTime(((Option)CommandOptions["end"]).Value);}
            set {SetCommandOption("end", String.Format(CultureInfo.InvariantCulture,"-D \"{0}\"", DateParser.GetCvsDateString(value)), true);}
        }

        /// <summary>
        /// The xsl style sheet to apply to the cvs log.
        /// </summary>
        [TaskAttribute("xslfile", Required=false)]
        public string Xsl {
            get {return ((Option)CommandOptions["xsl"]).Value;}
            set {SetCommandOption("xsl", String.Format(CultureInfo.InvariantCulture,"-xsl \"{0}\"", value), true);}
        }

        #endregion
    }
}
