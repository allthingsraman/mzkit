﻿// export R# package module type define for javascript/typescript language
//
//    imports "hmdb_kit" from "mzkit";
//
// ref=mzkit.HMDBTools@mzkit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null

/**
*/
declare namespace hmdb_kit {
   /**
     * @param env default value Is ``null``.
   */
   function biospecimen_slicer(hmdb: object, locations: object, env?: object): any;
   /**
   */
   function chemical_taxonomy(metabolite: object): string;
   module export {
      /**
        * @param file default value Is ``null``.
        * @param env default value Is ``null``.
      */
      function hmdb_table(hmdb: object, file?: any, env?: object): any;
   }
   /**
     * @param cache_dir default value Is ``'./hmdb/'``.
     * @param tabular default value Is ``false``.
     * @param env default value Is ``null``.
   */
   function get_hmdb(id: string, cache_dir?: string, tabular?: boolean, env?: object): object|object;
   module read {
      /**
      */
      function hmdb(xml: string): object;
      /**
        * @param hmdbRaw default value Is ``false``.
        * @param lazy default value Is ``true``.
        * @param env default value Is ``null``.
      */
      function hmdb_spectrals(repo: string, hmdbRaw?: boolean, lazy?: boolean, env?: object): any;
   }
}
