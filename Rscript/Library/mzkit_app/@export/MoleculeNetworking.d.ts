﻿// export R# package module type define for javascript/typescript language
//
//    imports "MoleculeNetworking" from "mzDIA";
//
// ref=mzkit.MoleculeNetworking@mzDIA, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

/**
 * Molecular Networking (MN) is a computational strategy that may help visualization and interpretation of the complex data arising from MS analysis.
 * 
 * > MN is able to identify potential similarities among all MS/MS spectra within 
 * >  the dataset and to propagate annotation to unknown but related molecules 
 * >  (Wang et al., 2016). This approach exploits the assumption that structurally
 * >  related molecules produce similar fragmentation patterns, and therefore they 
 * >  should be related within a network (Quinn et al., 2017). In MN, MS/MS data 
 * >  are represented in a graphical form, where each node represents an ion with 
 * >  an associated fragmentation spectrum; the links among the nodes indicate 
 * >  similarities of the spectra. By propagation of the structural information within
 * >  the network, unknown but structurally related molecules can be highlighted
 * >  and successful dereplication can be obtained (Yang et al., 2013); this may
 * >  be particularly useful for metabolite and NPS identification.
 * >  
 * >  MN has been implemented In different fields, particularly metabolomics And 
 * >  drug discovery (Quinn et al., 2017); MN In forensic toxicology was previously
 * >  used by Allard et al. (2019) For the retrospective analysis Of routine 
 * >  cases involving biological sample analysis. Yu et al. (2019) also used MN 
 * >  analysis For the detection Of designer drugs such As NBOMe derivatives And 
 * >  they showed that unknown compounds could be recognized As NBOMe-related 
 * >  substances by MN.
 * >  
 * >  In the present work the Global Natural Products Social platform (GNPS) was 
 * >  exploited to analyze HRMS/MS data obtained from the analysis of seizures 
 * >  collected by the Italian Department of Scientific Investigation of Carabinieri 
 * >  (RIS). The potential of MN to highlight And support the identification of
 * >  unknown NPS belonging to chemical classes such as fentanyls And synthetic
 * >  cannabinoids has been demonstrated.
*/
declare namespace MoleculeNetworking {
   module as {
      /**
       * convert the cluster tree into the graph model
       * 
       * 
        * @param tree A cluster tree which is created via the ``tree`` function.
        * @param ions the source data for create the cluster tree
      */
      function graph(tree: object, ions: object): object;
   }
   /**
    * Do spectrum clustering on a small bundle of the ms2 spectrum from a single raw data file
    * 
    * > this workflow usually used for processing the ms2 spectrum inside a 
    * >  single raw data file
    * 
     * @param ions -
     * @param mzdiff1 the mzdiff tolerance value for group the ms2 spectrum via the precursor m/z,
     *  for precursor m/z comes from the ms1 deconvolution peaktable, tolerance error
     *  should be smaller in ppm unit; 
     *  for precursor m/z comes from the ms2 parent ion m/z, tolerance error should 
     *  be larger in da unit.
     * 
     * + default value Is ``'da:0.1'``.
     * @param mzdiff2 the mzdiff tolerance value for do ms2 peak centroid or peak matches for do the
     *  cos similarity score evaluation, should be larger tolerance value in unit da,
     *  value of this tolerance parameter could be da:0.3
     * 
     * + default value Is ``'da:0.3'``.
     * @param intocutoff intensity cutoff value for make spectrum centroid
     * 
     * + default value Is ``0.05``.
     * @param tree_identical score cutoff for assert that spectrum in the binary tree
     *  is in the same cluster node
     * 
     * + default value Is ``0.8``.
     * @param tree_right score cutoff for assert that spectrum in the binary tree should be put into the right
     *  node.
     * 
     * + default value Is ``0.01``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function clustering(ions: any, mzdiff1?: any, mzdiff2?: any, intocutoff?: number, tree_identical?: number, tree_right?: number, env?: object): object;
   /**
    * Make precursor assigned to the cluster node
    * 
    * 
     * @param grid -
     * @param peakset -
     * @param assign_top 
     * + default value Is ``3``.
   */
   function grid_assigned(grid: object, peakset: object, assign_top?: object): object;
   /**
    * populate a list of peak ms2 cluster data
    * 
    * 
     * @param tree -
     * @param ions -
     * @return a set of ms2 data groups, in format of ``[guid => peakms2]`` vector tuples
   */
   function msBin(tree: object, ions: object): object;
   /**
    * create representative spectrum data
    * 
    * > @``P:BioNovoGene.Analytical.MassSpectrometry.Math.Spectra.PeakMs2.collisionEnergy`` is tagged as the cluster size
    * 
     * @param mzdiff -
     * 
     * + default value Is ``'da:0.3'``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function representative(tree: object, mzdiff?: any, env?: object): object;
   /**
    * Create grid clustering of the ms2 spectrum data
    * 
    * 
     * @param rawdata -
     * @param centroid -
     * 
     * + default value Is ``'da:0.3'``.
     * @param intocutoff -
     * 
     * + default value Is ``0.05``.
     * @param rt_win 
     * + default value Is ``20``.
     * @param dia_n set this decompose parameter to any positive integer value greater 
     *  than 1 may produce too many data for analysis, make the workflow 
     *  too slow.
     * 
     * + default value Is ``-1``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function spectrum_grid(rawdata: any, centroid?: any, intocutoff?: number, rt_win?: number, dia_n?: object, env?: object): any;
   /**
    * Split each cluster data into multiple parts by a givne rt window
    * 
    * > This function works for the small molecular networking analysis
    * 
     * @param clusters -
     * @param rtwin -
     * 
     * + default value Is ``30``.
     * @param wrap_peaks wraping the networking node data as the spectrum peak object?
     * 
     * + default value Is ``false``.
     * @param env -
     * 
     * + default value Is ``null``.
     * @return the value type of this function is affects by the **`wrap_peaks`** parameter:
     *  
     *  1. for wrap_peaks is set to false by default, a vector of the raw @``T:BioNovoGene.Analytical.MassSpectrometry.Math.Spectra.MoleculeNetworking.NetworkingNode`` 
     *     which is extract from the cluster data will be returns
     *  2. otherwise the spectrum peaks data will be returns if the parameter 
     *     value is set to value true.
   */
   function splitClusterRT(clusters: any, rtwin?: number, wrap_peaks?: boolean, env?: object): object|object;
   /**
    * do spectrum data clustering
    * 
    * 
     * @param ions A set of the spectrum data
     * @param mzdiff the ms2 fragment mass tolerance when used for compares 
     *  ms2 spectrum data
     * 
     * + default value Is ``0.3``.
     * @param intocutoff intensity cutoff value that used for make the spectrum 
     *  centroid and noise cleanup
     * 
     * + default value Is ``0.05``.
     * @param equals -
     * 
     * + default value Is ``0.85``.
   */
   function tree(ions: object, mzdiff?: number, intocutoff?: number, equals?: number): object;
   /**
    * makes the spectrum data its unique id reference uniqued!
    * 
    * 
     * @param ions A collection of the mzkit spectrum object
   */
   function uniqueNames(ions: object): object;
   /**
    * Unpack of the spectrum data into multiple file groups
    * 
    * 
     * @param assign -
     * @param env -
     * 
     * + default value Is ``null``.
     * @return A tuple list of the spectrum data in multiple file groups, 
     *  each slot tuple is a rawdata file content.
   */
   function unpack_assign(assign: any, env?: object): any;
   /**
   */
   function unpack_unmapped(grid: object): any;
}
