﻿// export R# package module type define for javascript/typescript language
//
//    imports "mzPack" from "mzkit";
//
// ref=mzkit.MzPackAccess@mzkit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

/**
 * raw data accessor for the mzpack data object
 * 
*/
declare namespace mzPack {
   /**
    * method for write mzpack data object as a mzXML file
    * 
    * 
     * @param mzpack -
     * @param file the file stream to the target mzXML file to write the data
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function convertTo_mzXML(mzpack: object, file: any, env?: object): boolean;
   /**
    * get all of the sample file data tags from target mzpack file
    * 
    * 
     * @param mzpack -
     * @param env 
     * + default value Is ``null``.
   */
   function getSampleTags(mzpack: any, env?: object): string;
   /**
    * Get object list inside the MS packdata
    * 
    * > show all ms1 scan id in a mzpack data object or 
    * >  show all raw data file names in a mzwork data 
    * >  package.
    * 
     * @param mzpack -
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function ls(mzpack: any, env?: object): string;
   /**
    * get metadata list from a specific ms1 scan
    * 
    * 
     * @param mzpack A mzpack data lazy reader object that created via ``mzpack`` function.
     * @param index the scan id of the target ms1 scan data
   */
   function metadata(mzpack: object, index: string): object;
   /**
    * open a mzpack data object reader, not read all data into memory in one time.
    * 
    * > a in-memory reader wrapper will be created if the given file object 
    * >  is a in-memory mzpack object itself
    * 
     * @param file the file path for the mzpack file or the mzpack data object it self
     * @param env -
     * 
     * + default value Is ``null``.
     * @return the ms scan data can be load into memory in lazy 
     *  require by a given scan id of the target ms1 scan
   */
   function mzpack(file: any, env?: object): object;
   /**
    * open a mzwork package file
    * 
    * 
     * @param file -
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function mzwork(file: any, env?: object): object;
   module open {
      /**
       * open mzwork file and then populate all of the mzpack raw data file
       * 
       * 
        * @param mzwork -
        * @param env 
        * + default value Is ``null``.
        * @return a collection of mzpack raw data objects
      */
      function mzwork(mzwork: string, env?: object): object;
   }
   /**
    * pack a given ms2 spectrum collection as a single ms1 scan product
    * 
    * 
     * @param ms2 a collection of the ms2 spectrum data
     * @param scan_id1 the scan id which will be specificed to the result scan ms1 object
     * 
     * + default value Is ``null``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function pack_ms1(ms2: any, scan_id1?: string, env?: object): object;
   /**
    * pack mzkit ms2 peaks data as a mzpack data object
    * 
    * 
     * @param data A collection of the scan ms1 or ms2 data for pack as mzpack object
     * @param timeWindow the time slide window size for create different
     *  data scan groups in the mzpack object. this rt data grouping window parameter is not working
     *  when the input ms spectrum data collection is scan ms1 objects
     * 
     * + default value Is ``1``.
     * @param pack_singleCells 
     * + default value Is ``false``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function packData(data: any, timeWindow?: number, pack_singleCells?: boolean, env?: object): object;
   /**
    * write mzPack in v2 format
    * 
    * 
     * @param data -
     * @param file A file path that reference to the local file stream for save the data
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function packStream(data: object, file: any, env?: object): any;
   /**
    * ### get mzpack object from mzwork archive
    *  
    *  read mzpack data from the mzwork package by a 
    *  given raw data file name as reference id
    * 
    * 
     * @param mzwork a zip archive liked data package, contains multiple mzpack object
     * @param fileName the reference key for extract the mzpack data
     * @param single 
     * + default value Is ``false``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function readFileCache(mzwork: object, fileName: string, single?: boolean, env?: object): object;
   /**
    * Removes the sciex AB5600 noise data from the MS2 raw data
    * 
    * 
     * @param raw should be a mzpack object or a collection of the ms2 data for handling the noise spectra peak removes
     * @param cut 
     * + default value Is ``2``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function removeSciexNoise(raw: any, cut?: object, env?: object): any;
   /**
    * get ms scan information metadata list
    * 
    * 
     * @param mzpack A mzpack data lazy reader object that created via ``mzpack`` function.
     * @param index the scan id of the target ms1 scan data
   */
   function scaninfo(mzpack: object, index: string): object;
   /**
    * try to split target mzpack file into multiple parts based on the sample tags
    * 
    * 
     * @param mzpack -
   */
   function split_samples(mzpack: string): any;
}
