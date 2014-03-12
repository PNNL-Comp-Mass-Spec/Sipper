SIPPER can be used to automatically detect and quantify partially labeled C13 peptides 
in a single dataset.  It includes a GUI for manual visualization and annotation of 
detected LC-MS features. It also includes a Simple MS Viewer for exploring mass 
spectral data. Supports Thermo .Raw files, plus also mzXML, mzML, and mz5.

In order to read Thermo .raw files, you must install the Thermo MSFileReader v2.2 from
http://sjsupport.thermofinnigan.com/public/detail.asp?id=703
When the installer offers you the option of the version to install, be sure to install the 32-bit version.
You can also install the 64-bit version, though SIPPER only uses the 32-bit DLLs.


Analysis Steps

1) Copy the Thermo .Raw file to a local folder
2) Customize your parameter file, e.g. SipperTargetedWorkflowParameters1.xml
3) Define the search targets, e.g. Yellow_C13_070_targets.txt
	- Required columns are:
		TargetID
		EmpiricalFormula
		ChargeState
		Scan

	- Optional columns include:
		Dataset
		MassTagID

	- Note that the Scan number of the target will be matched to the data in the comparison dataset 
	  using ChromNETTolerance, which is a value between 0 and 1. For example, a ChromNETTolerance 
	  of 0.025 means +/- 2.5% in Normalized Elution Time, where NET is determined by transforming
	  the observed scan numbers to a range of 0 to 1

4) Start SIPPER
5) Click AutoProcess
6) Define the paths to the input files
	Raw data file:   Yellow_C13_070_23Mar10_Griffin_10-01-28.raw
	Parameter file:  SipperTargetedWorkflowParameters1.xml
	Targets:         Yellow_C13_070_targets.txt
7) Click Go
8) Once processing is complete, click the X on the upper right of the Autoprocessor Window
9) Now click "View and Annotate"
	- The program should have automatically loaded the results


-------------------------------------------------------------------------------
Written by Gordon Slysz for the Department of Energy (PNNL, Richland, WA)

E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
Website: http://panomics.pnnl.gov/ or http://omics.pnl.gov
-------------------------------------------------------------------------------

Licensed under the Apache License, Version 2.0; you may not use this file except 
in compliance with the License.  You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0

All publications that result from the use of this software should include 
the following acknowledgment statement:
 Portions of this research were supported by the W.R. Wiley Environmental 
 Molecular Science Laboratory, a national scientific user facility sponsored 
 by the U.S. Department of Energy's Office of Biological and Environmental 
 Research and located at PNNL.  PNNL is operated by Battelle Memorial Institute 
 for the U.S. Department of Energy under contract DE-AC05-76RL0 1830.

Notice: This computer software was prepared by Battelle Memorial Institute, 
hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the 
Department of Energy (DOE).  All rights in the computer software are reserved 
by DOE on behalf of the United States Government and the Contractor as 
provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY 
WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS 
SOFTWARE.  This notice including this sentence must appear on any copies of 
this computer software.
