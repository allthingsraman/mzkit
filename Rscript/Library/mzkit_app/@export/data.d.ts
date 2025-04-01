﻿// export R# package module type define for javascript/typescript language
//
//    imports "data" from "mzkit";
//
// ref=mzkit.data@mzkit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

/**
 * m/z data operator module
 * 
*/
declare namespace data {
   /**
    * get alignment result tuple: query and reference
    * 
    * 
     * @param align -
     * @param query 
     * + default value Is ``'Query'``.
     * @param reference 
     * + default value Is ``'Reference'``.
     * @return a tuple list object that contains spectrum alignment result:
     *  
     *  1. query - spectrum of sample query
     *  2. reference - spectrum of library reference
   */
   function alignment_ref(align: object, query?: string, reference?: string): object;
   /**
    * Make alignment string
    * 
    * 
     * @param mz -
     * @param query -
     * @param reference -
     * @param annotation -
     * 
     * + default value Is ``null``.
   */
   function alignment_str(mz: any, query: any, reference: any, annotation?: any): string;
   /**
    * make a tuple list via grouping of the spectrum data via the ROI id inside the metadata list
    * 
    * 
     * @param peakms2 a collection of the spectrum data to make spectrum data grouping.
     * @param default the default ROI id for make the data groups if the metadata 
     *  is null or the ``ROI`` id tag is missing from the spectrum object metadata.
     * 
     * + default value Is ``'Not_Assigned'``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function groupBy_ROI(peakms2: any, default?: string, env?: object): any;
   /**
    * get intensity value from the ion scan points
    * 
    * 
     * @param ticks -
     * @param mz 
     * + default value Is ``-1``.
     * @param mzdiff 
     * + default value Is ``'da:0.3'``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function intensity(ticks: any, mz?: number, mzdiff?: any, env?: object): number;
   /**
    * Create a library matrix object
    * 
    * 
     * @param matrix for a dataframe object, should contains column data:
     *  mz, into and annotation.
     * 
     * + default value Is ``null``.
     * @param title -
     * 
     * + default value Is ``'MS Matrix'``.
     * @param parentMz 
     * + default value Is ``-1``.
     * @param centroid 
     * + default value Is ``false``.
     * @param args 
     * + default value Is ``null``.
     * @param env -
     * 
     * + default value Is ``null``.
     * @return A simple mzkit spectrum peak list object
   */
   function libraryMatrix(matrix?: any, title?: string, parentMz?: number, centroid?: boolean, args?: object, env?: object): any;
   /**
     * @param topIons default value Is ``5``.
   */
   function linearMatrix(data: object, topIons?: object): string;
   module make {
      /**
       * makes xcms_id format liked ROI unique id
       * 
       * 
        * @param ROIlist -
        * @param name_chrs just returns the ROI names character?
        * 
        * + default value Is ``false``.
        * @param prefix 
        * + default value Is ``''``.
        * @param env -
        * 
        * + default value Is ``null``.
      */
      function ROI_names(ROIlist: any, name_chrs?: boolean, prefix?: string, env?: object): object;
   }
   /**
    * get the size of the target ms peaks
    * 
    * 
     * @param matrix -
     * @param env 
     * + default value Is ``null``.
   */
   function nsize(matrix: any, env?: object): object;
   /**
    * create a new ms2 peaks data object
    * 
    * 
     * @param precursor -
     * @param rt -
     * @param mz -
     * @param into -
     * @param totalIons -
     * 
     * + default value Is ``0``.
     * @param file -
     * 
     * + default value Is ``null``.
     * @param libname 
     * + default value Is ``null``.
     * @param precursor_type 
     * + default value Is ``null``.
     * @param meta -
     * 
     * + default value Is ``null``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function peakMs2(precursor: number, rt: number, mz: number, into: number, totalIons?: number, file?: string, libname?: string, precursor_type?: string, meta?: object, env?: object): object;
   module read {
      /**
      */
      function MsMatrix(file: string): object;
   }
   /**
    * Create a representative spectrum from a given spectrum collection via a sum or mean aggregate method.
    * 
    * 
     * @param x -
     * @param mean -
     * 
     * + default value Is ``true``.
     * @param centroid 
     * + default value Is ``0.1``.
     * @param intocutoff 
     * + default value Is ``0.05``.
     * @param env 
     * + default value Is ``null``.
   */
   function representative(x: any, mean?: boolean, centroid?: number, intocutoff?: number, env?: object): object;
   /**
    * slice a region of ms1 scan data by a given rt window.
    * 
    * 
     * @param ms1 a sequence of ms1 scan data.
     * @param rtmin -
     * @param rtmax -
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function rt_slice(ms1: any, rtmin: number, rtmax: number, env?: object): object;
   /**
    * get scan time value from the ion scan points
    * 
    * 
     * @param ticks -
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function scan_time(ticks: any, env?: object): number;
   /**
    * search the target query spectra against a reference mzpack data file
    * 
    * 
     * @param q The target spectra data, mz and into data fields must 
     *  be included inside if this parameter value is a dataframe.
     * @param refer A mzpack data object that contains the reference 
     *  spectrum dataset. The spectra dataset inside this mzpack data object
     *  must be already been centroid processed!
     * @param tolerance 
     * + default value Is ``'da:0.3'``.
     * @param intocutoff 
     * + default value Is ``0.05``.
     * @param similarity_cutoff 
     * + default value Is ``0.3``.
     * @param env 
     * + default value Is ``null``.
   */
   function search(q: any, refer: object, tolerance?: any, intocutoff?: number, similarity_cutoff?: number, env?: object): object;
   /**
    * evaluate the splash id of the given spectrum data
    * 
    * > The SPLASH is an unambiguous, database-independent spectral identifier, 
    * >  just as the InChIKey is designed to serve as a unique identifier for 
    * >  chemical structures. It contains separate blocks that define different 
    * >  layers of information, separated by dashes. For example, the full SPLASH 
    * >  of a caffeine mass spectrum above is splash10-0002-0900000000-b112e4e059e1ecf98c5f.
    * >  The first block is the SPLASH identifier, the second and third are 
    * >  summary blocks, and the fourth is the unique hash block.
    * >  
    * >  The SPLASH began As the MoNA (Massbank Of North America) hash, designed To 
    * >  identify duplicate spectra within the database. This idea developed further 
    * >  during the 2015 Metabolomics conference, where the SPLASH collaboration 
    * >  was formed. Currently, the specification has been formalized For mass 
    * >  spectrometry data. Additional specifications For IR, UV And NMR spectrometry
    * >  are planned.
    * 
     * @param spec -
     * @param type 
     * + default value Is ``null``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function splash_id(spec: any, type?: object, env?: object): any;
   /**
    * Union and merge the given multiple spectrum data into one single spectrum
    * 
    * 
     * @param peaks A collection of the spectrum object that going to merge into single one
     * @param matrix this parameter will affects the data type of the value returns of this function:
     *  
     *  1. default false, returns a peak ms2 data object
     *  2. true, returns a library matrix data object
     * 
     * + default value Is ``false``.
     * @param massDiff the mass error for merge two spectra peak
     * 
     * + default value Is ``0.1``.
     * @return a single ms spectrum data object, its data type depeneds on the **`matrix`** parameter.
   */
   function unionPeaks(peaks: object, matrix?: boolean, massDiff?: number): object|object;
   /**
    * get chromatogram data for a specific metabolite with given m/z from the ms1 scans data.
    * 
    * 
     * @param ms1 a sequence data of ms1 scans, or the mzkit mzpack data object.
     * @param mz target mz value
     * @param tolerance tolerance value in unit ``ppm`` or ``da`` for 
     *  extract ``m/z`` data from the given ms1 ion 
     *  scans.
     * 
     * + default value Is ``'ppm:20'``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function XIC(ms1: any, mz: number, tolerance?: any, env?: object): object|object|object;
   /**
    * grouping of the ms1 scan points by m/z data
    * 
    * 
     * @param ms1 -
     * @param tolerance the m/z diff tolerance value for grouping ms1 scan point 
     *  based on its ``m/z`` value
     * 
     * + default value Is ``'ppm:20'``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function XIC_groups(ms1: any, tolerance?: any, env?: object): any;
}
