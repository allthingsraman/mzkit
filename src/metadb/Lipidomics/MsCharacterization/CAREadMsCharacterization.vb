﻿Friend Class CAREadMsCharacterization
    Public Shared Function Characterize(scan As IMSScanProperty, molecule As ILipid, reference As MoleculeMsReference, tolerance As Single, mzBegin As Single, mzEnd As Single) As (ILipid, Double())

        Dim defaultResult = EieioMsCharacterizationUtility.GetDefaultScore(scan, reference, tolerance, mzBegin, mzEnd, 2, 1, 0, 0.5)
        Return GetDefaultCharacterizationResultForSingleAcylChainLipid(molecule, defaultResult)
    End Function
End Class