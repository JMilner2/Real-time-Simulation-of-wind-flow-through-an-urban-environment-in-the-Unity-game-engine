# Real-time Simulation of wind flow through an urban environment in Unity.

Joshua Milner (210181083)

May 2024

BSc. Computer Science

Supervisor - Giacomo Bergami

Word Count: 14,365

# Abstract

The study of wind flow through an urban area is integral to help architects and engineers better understand the wind patterns their buildings produce. Computational fluid dynamics (CFD) software is often used to simulate wind flow, but it demands a previous technical understanding and requires extensive computational resources or time. This project aims to address these challenges by developing an accessible, real-time wind flow simulator in the Unity game engine. By adapting a previous CFD solver, this wind simulator allows users to place building and easily interpret wind flow through visualisations. Validation is done on the simulations results against wind-tunnel data to ensures accurate wind flow is being produced. Testing and optimisation of the algorithms ensures they remain efficient and accessible. This dissertation enables architects to simulate wind flow through an urban area quickly, and without the complexities of current CFD software.

# Declaration

“I hereby declare that this dissertation represents my own work, except where  
otherwise stated.”

# Acknowledgements

I would like to express my profound appreciation to my supervisor, Giacomo Bergami. His intellectual rigor, insightful questions, and commitment to the development of this project pushed me to achieve my full potential in this dissertation.

I would also like to thank Daisy-Grace Dobbing for her support and motivation throughout this dissertation.

Table of Contents

[Abstract 2](#_Toc166011281)

[Declaration 3](#_Toc166011282)

[Acknowledgements 4](#_Toc166011283)

[Introduction 7](#_Toc166011284)

[Context for the project 7](#_Toc166011285)

[Definition of the problem 7](#_Toc166011286)

[Proposed solution 8](#_Toc166011287)

[Aims and Objectives 8](#_Toc166011288)

[Dissertation structure 9](#_Toc166011289)

[Technical Background 10](#_Toc166011290)

[Current techniques for visualising wind flow 10](#_Toc166011291)

[SimScale 10](#_Toc166011292)

[M. B. a. V. Cristie, “CFD Post-processing in Unity3D” 11](#_Toc166011293)

[S. Schneider, “EXPLORING SCHEMES FOR VISUALIZING URBAN WIND FIELDS BASED ON CFD SIMULATIONS BY EMPLOYING OGC STANDARDS” 11](#_Toc166011294)

[Current papers simulating wind flow in an urban area 11](#_Toc166011295)

[K. Tsai, “Evaluating the Influence of Urban Morphology on Urban Wind Environment Based on Computational Fluid Dynamics Simulation” 12](#_Toc166011296)

[Z. Kaseb, “A framework for pedestrian-level wind conditions improvement in urban areas: CFD simulation and optimization” 12](#_Toc166011297)

[Y. Xiao, et al., "Effects of inflow conditions on mountainous/urban wind environment simulation" 13](#_Toc166011298)

[M. Tutar And G. Oguz, "Computational modelling of wind flow around a group of buildings” 13](#_Toc166011299)

[Jon Stam “Real-Time Fluid Dynamics for Games” 14](#_Toc166011300)

[Choice of fluid solver 14](#_Toc166011301)

[Why Computational fluid dynamics is necessary 16](#_Toc166011302)

[Implementation 17](#_Toc166011303)

[Ethical considerations 17](#_Toc166011304)

[Fluid simulation 17](#_Toc166011305)

[Fluid solver - Main loop 19](#_Toc166011306)

[Advect 20](#_Toc166011307)

[Diffuse 21](#_Toc166011308)

[Project 23](#_Toc166011309)

[Add forces 24](#_Toc166011310)

[Boundary conditions 26](#_Toc166011311)

[Visualisation techniques 29](#_Toc166011312)

[Wind speed and direction texture 29](#_Toc166011313)

[Wind speed texture 30](#_Toc166011314)

[Wind Direction with gizmos 32](#_Toc166011315)

[Results and Evaluation 35](#_Toc166011316)

[Grid density testing and evaluation 35](#_Toc166011317)

[Validating the wind simulation 37](#_Toc166011318)

[Building density testing and evaluation 39](#_Toc166011319)

[Building density effect on simulation runtime 40](#_Toc166011320)

[Building density effect on memory usage 42](#_Toc166011321)

[Building density effect on simulation accuracy 43](#_Toc166011322)

[Correctness testing the 2D and 3D wind simulations 44](#_Toc166011323)

[Conclusion 47](#_Toc166011324)

[Future work 47](#_Toc166011325)

[Summary 48](#_Toc166011326)

[Bibliography 49](#_Toc166011327)

Please see upload file for full diss
