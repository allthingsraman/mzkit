﻿// export R# package module type define for javascript/typescript language
//
// ref=mzkit.MetaDbXref@mzkit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

/**
 * Metabolite annotation database search engine
 * 
*/
declare namespace metadb {
   /**
     * @param env default value Is ``null``.
   */
   function annotationStream(id: string, name: string, formula: string, env?: object): object;
   module cbind {
      /**
        * @param env default value Is ``null``.
      */
      function metainfo(anno: object, engine: any, env?: object): any;
   }
   /**
    * removes all of the annotation result which is not 
    *  hits in the given ``id`` set.
    * 
    * 
     * @param query -
     * @param id the required compound id set that should be hit!
     * @param field -
     * @param metadb -
     * @param includes_metal_ions removes metabolite annotation result which has metal
     *  ions inside formula string by default.
     * 
     * + default value Is ``false``.
     * @param excludes reverse the logical of select the annotation result 
     *  based on the given **`id`** set.
     * 
     * + default value Is ``false``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function excludeFeatures(query: object, id: string, field: string, metadb: object, includes_metal_ions?: boolean, excludes?: boolean, env?: object): object;
   /**
    * get metabolite annotation metadata by a set of given unique reference id
    * 
    * 
     * @param engine -
     * @param uniqueId -
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function getMetadata(engine: any, uniqueId: object, env?: object): any;
   /**
     * @param env default value Is ``null``.
   */
   function load_asQueryHits(x: object, env?: object): object;
   /**
    * a generic function for handle ms1 search
    * 
    * 
     * @param compounds kegg compounds
     * @param precursors -
     * @param tolerance -
     * 
     * + default value Is ``'ppm:20'``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function ms1_handler(compounds: any, precursors: any, tolerance?: any, env?: object): any;
   /**
    * get duplictaed raw annotation results.
    * 
    * 
     * @param engine -
     * @param mz a m/z numeric vector or a object list that 
     *  contains the data mapping of unique id to 
     *  m/z value.
     * @param unique 
     * + default value Is ``false``.
     * @param uniqueByScore only works when **`unique`** parameter
     *  value is set to value TRUE.
     * 
     * + default value Is ``false``.
     * @param env 
     * + default value Is ``null``.
   */
   function ms1_search(engine: any, mz: any, unique?: boolean, uniqueByScore?: boolean, env?: object): object;
   /**
     * @param keepsRaw default value Is ``false``.
     * @param env default value Is ``null``.
   */
   function parseLipidName(name: any, keepsRaw?: boolean, env?: object): any;
   /**
   */
   function precursorIon(ion: string): object;
   /**
    * Found the best matched mz value with the target **`exactMass`**
    * 
    * 
     * @param mz -
     * @param exactMass -
     * @param adducts -
     * @param mzdiff -
     * 
     * + default value Is ``'da:0.005'``.
     * @param env 
     * + default value Is ``null``.
     * @return function returns a evaluated mz under the specific **`adducts`** value
     *  and it also the min mass tolerance, if no result has mass tolerance less then the 
     *  given threshold value, then this function returns nothing
   */
   function searchMz(mz: any, exactMass: number, adducts: object, mzdiff?: any, env?: object): object;
   /**
    * unique of the peak annotation features
    * 
    * 
     * @param query all query result that comes from the ms1_search function.
     * @param uniqueByScore 
     * + default value Is ``false``.
     * @param scoreFactors the reference name this score data must be 
     *  generated via the @``M:BioNovoGene.BioDeep.MSEngine.MzQuery.ReferenceKey(BioNovoGene.BioDeep.MSEngine.MzQuery,System.String)`` 
     *  function.
     * 
     * + default value Is ``null``.
     * @param format the numeric format of the mz value for generate the reference key
     * 
     * + default value Is ``'F4'``.
     * @param removesZERO removes all metabolites with ZERO score?
     * 
     * + default value Is ``false``.
     * @param verbose 
     * + default value Is ``false``.
     * @param env -
     * 
     * + default value Is ``null``.
   */
   function uniqueFeatures(query: object, uniqueByScore?: boolean, scoreFactors?: object, format?: string, removesZERO?: boolean, verbose?: boolean, env?: object): object;
   /**
     * @param env default value Is ``null``.
   */
   function verify_cas_number(num: any, env?: object): any;
}