# SIPPER

SIPPER can be used to automatically detect and quantify partially labeled C13 peptides 
in a single dataset.  It includes a GUI for manual visualization and annotation of 
detected LC-MS features. It also includes a Simple MS Viewer for exploring mass 
spectral data. Supports Thermo .Raw files, plus also mzXML, mzML, and mz5.
In addition, there is a console version available for batch processing.

For algorithm details, see manuscript "Automated data extraction from in situ protein-stable isotope probing studies.",
J Proteome Res. 2014 Mar 7;13(3):1200-10. \
[Abstract on PubMed](https://www.ncbi.nlm.nih.gov/pubmed/24467184)

## Requirements

Uses ThermoFisher.CommonCore.RawFileReader.dll to read Thermo .raw files.

Uses Proteowizard to read .mzML files
* Download the Windows 64-bit installer from https://proteowizard.sourceforge.io/

## Analysis Steps

1) Copy the Thermo .Raw file to a local folder
2) Customize your parameter file, e.g. SipperTargetedWorkflowParameters1.xml
3) Define the search targets, e.g. Yellow_C13_070_targets.txt
* Required columns:
  * TargetID
  * EmpiricalFormula
  * ChargeState
  * Scan
* Optional columns:
  * Dataset
  * MassTagID

Note that the scan number of the target will be matched to the data in the comparison dataset 
using ChromNETTolerance, which is a value between 0 and 1. For example, a ChromNETTolerance 
of 0.025 means +/- 2.5% in Normalized Elution Time, where NET is determined by transforming
the observed scan numbers to a range of 0 to 1.

4) Start SIPPER
5) Click AutoProcess
6) Define the paths to the input files

| Description    | Filename                                    |
|----------------|---------------------------------------------|
| Raw data file  | Yellow_C13_070_23Mar10_Griffin_10-01-28.raw |
| Parameter file | SipperTargetedWorkflowParameters1.xml       |
| Targets:       | Yellow_C13_070_targets.txt                  |

7) Click Go
8) Once processing is complete, click the X on the upper right of the Autoprocessor Window
9) Now click "View and Annotate"
* The program should have automatically loaded the results

## Console Version

Program `Sipper_Console.exe` can be used on the command line to batch process files with SIPPER.
This program works on both Windows and Linux, though on Linux you invoke it with 
[Mono](https://www.mono-project.com/)

### Sipper_Console syntax

```
Sipper_Console.exe
 -I:DatasetFileOrDirectoryPath
 -P:ParameterFilePath
 -T:TargetsFilePath
 [-O:OutputDirectoryPath]
 [-ParamFile:ParamFileName.conf] [-CreateParamFile]
```

Use `-I` to specify the dataset file path

Use `-P` to specify the SIPPER parameter file path

Use `-T` to specify the targets file path
* As described above, the file must have columns TargetID, EmpiricalFormula, ChargeState, and Scan
* It can optionally have columns Dataset and MassTagID

Optionally use `-O` to specify the directory for saving results
* SIPPER will write results to a subdirectory named Results, below the path specified by -O
  * Filename: DatasetName_results.txt
* SIPPER will create a log ifle in a subdirectory named Logs, below the path specified by -O
  * Filename: DatasetName_log.txt
* If `-O` is not provided, results will be saved in subdirectories below the directory with the dataset file

The processing options can be specified in a parameter file using `-ParamFile:Options.conf`
* Define options using the format `ArgumentName=Value`
* Lines starting with `#` or `;` will be treated as comments
* Additional arguments on the command line can supplement or override the arguments in the parameter file

Use `-CreateParamFile` to create an example parameter file
* By default, the example parameter file content is shown at the console
* To create a file named Options.conf, use `-CreataParamFile:Options.conf`

## Contacts

Written by Gordon Slysz and Matthew Monroe for the Department of Energy (PNNL, Richland, WA) \
E-mail: proteomics@pnnl.gov \
Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics

## License

SIPPER is licensed under the Apache License, Version 2.0; you may not use this 
file except in compliance with the License.  You may obtain a copy of the 
License at https://opensource.org/licenses/Apache-2.0

RawFileReader reading tool. Copyright © 2016 by Thermo Fisher Scientific, Inc. All rights reserved.
