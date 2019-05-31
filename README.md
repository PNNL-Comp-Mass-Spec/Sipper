# SIPPER

SIPPER can be used to automatically detect and quantify partially labeled C13 peptides 
in a single dataset.  It includes a GUI for manual visualization and annotation of 
detected LC-MS features. It also includes a Simple MS Viewer for exploring mass 
spectral data. Supports Thermo .Raw files, plus also mzXML, mzML, and mz5.

For algorithm details, see manuscript "Automated data extraction from in situ protein-stable isotope probing studies.",
J Proteome Res. 2014 Mar 7;13(3):1200-10. \
[Abstract on PubMed](https://www.ncbi.nlm.nih.gov/pubmed/?term=24467184)

## Requirements

Uses ThermoFisher.CommonCore.RawFileReader.dll to read Thermo .raw files.

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

Note that the Scan number of the target will be matched to the data in the comparison dataset 
using ChromNETTolerance, which is a value between 0 and 1. For example, a ChromNETTolerance 
of 0.025 means +/- 2.5% in Normalized Elution Time, where NET is determined by transforming
the observed scan numbers to a range of 0 to 1.

4) Start SIPPER
5) Click AutoProcess
6) Define the paths to the input files

| Description    | Filename |
|----------------|----------|
| Raw data file  | Yellow_C13_070_23Mar10_Griffin_10-01-28.raw |
| Parameter file | SipperTargetedWorkflowParameters1.xml |
| Targets:       |  Yellow_C13_070_targets.txt |

7) Click Go
8) Once processing is complete, click the X on the upper right of the Autoprocessor Window
9) Now click "View and Annotate"
* The program should have automatically loaded the results

## Contacts

Written by Gordon Slysz for the Department of Energy (PNNL, Richland, WA) \
E-mail: proteomics@pnnl.gov \
Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/

## License

SIPPER is licensed under the Apache License, Version 2.0; you may not use this 
file except in compliance with the License.  You may obtain a copy of the 
License at https://opensource.org/licenses/Apache-2.0

RawFileReader reading tool. Copyright © 2016 by Thermo Fisher Scientific, Inc. All rights reserved.
