﻿Public Class Omega3nChainGenerator
    Implements IChainGenerator
    Public Function CarbonIsValid(carbon As Integer) As Boolean Implements IChainGenerator.CarbonIsValid
        Return True
    End Function

    Public Function DoubleBondIsValid(carbon As Integer, doubleBond As Integer) As Boolean Implements IChainGenerator.DoubleBondIsValid
        Return carbon >= doubleBond * 3 + 3 AndAlso doubleBond >= 0
    End Function

    Private Shared ReadOnly eariestPositionOfOx As Integer = 2
    Public Function Generate(chain As AcylChain) As IEnumerable(Of IChain) Implements IChainGenerator.Generate
        Dim bs = EnumerateDoubleBond(chain.CarbonCount, chain.DoubleBond)
        Dim os = EnumerateOxidized(chain.CarbonCount, chain.Oxidized, eariestPositionOfOx, chain.CarbonCount - 1).ToArray()
        Return bs.SelectMany(Function(__) os, Function(b, o) New AcylChain(chain.CarbonCount, b, o))
    End Function

    Public Function Generate(chain As AlkylChain) As IEnumerable(Of IChain) Implements IChainGenerator.Generate
        Dim bs As IEnumerable(Of IDoubleBond)
        If chain.IsPlasmalogen Then
            bs = EnumerateDoubleBondInPlasmalogen(chain.CarbonCount, chain.DoubleBond)
        Else
            bs = EnumerateDoubleBondInEther(chain.CarbonCount, chain.DoubleBond)
        End If
        Dim os = EnumerateOxidized(chain.CarbonCount, chain.Oxidized, eariestPositionOfOx, chain.CarbonCount - 1).ToArray()
        Return bs.SelectMany(Function(__) os, Function(b, o) New AlkylChain(chain.CarbonCount, b, o))
    End Function

    Private Shared ReadOnly eariestPositionOfOxInSphingosine As Integer = 4
    Public Function Generate(chain As SphingoChain) As IEnumerable(Of IChain) Implements IChainGenerator.Generate
        Dim bs = EnumerateDoubleBondInSphingosine(chain.CarbonCount, chain.DoubleBond)
        Dim os = EnumerateOxidized(chain.CarbonCount, chain.Oxidized, eariestPositionOfOxInSphingosine, chain.CarbonCount - 1).ToArray()
        Return bs.SelectMany(Function(__) os, Function(b, o) New SphingoChain(chain.CarbonCount, b, o))
    End Function

    Private Function EnumerateDoubleBond(carbon As Integer, doubleBond As IDoubleBond) As IEnumerable(Of IDoubleBond)
        If doubleBond.UnDecidedCount = 0 Then
            Return [Return](doubleBond)
        End If
        If Not DoubleBondIsValid(carbon, doubleBond.Count) Then
            Return Enumerable.Empty(Of IDoubleBond)()
        End If
        Dim result = InnerEnumerateDoubleBond(carbon, doubleBond)
        If carbon = 14 AndAlso doubleBond.Count = 1 AndAlso doubleBond.UnDecidedCount = 1 Then
            result = result.Prepend(New DoubleBond(DoubleBondInfo.Create(9)))
        End If
        If carbon = 16 AndAlso doubleBond.Count = 1 AndAlso doubleBond.UnDecidedCount = 1 Then
            result = result.Prepend(New DoubleBond(DoubleBondInfo.Create(9)))
        End If
        If carbon = 15 AndAlso doubleBond.Count = 1 AndAlso doubleBond.UnDecidedCount = 1 Then
            result = result.Prepend(New DoubleBond(DoubleBondInfo.Create(10)))
        End If
        If carbon = 17 AndAlso doubleBond.Count = 1 AndAlso doubleBond.UnDecidedCount = 1 Then
            result = result.Prepend(New DoubleBond(DoubleBondInfo.Create(10)))
        End If
        Return result
    End Function

    Private Function InnerEnumerateDoubleBond(carbon As Integer, doubleBond As IDoubleBond) As IEnumerable(Of IDoubleBond)
        Dim [set] = New HashSet(Of Integer)(doubleBond.Bonds.[Select](Function(bond) bond.Position))
        For i = 0 To doubleBond.UnDecidedCount - 1
            [set].Add(carbon - i * 3)
        Next
        Return GenerateDoubleBonds([set], SetToBitArray(carbon, doubleBond), carbon - doubleBond.UnDecidedCount * 3, carbon)
    End Function

    Private Function EnumerateDoubleBondInEther(carbon As Integer, doubleBond As IDoubleBond) As IEnumerable(Of IDoubleBond)
        If doubleBond.UnDecidedCount = 0 Then
            Return [Return](doubleBond)
        End If
        If Not DoubleBondIsValid(carbon, doubleBond.Count - 1) Then
            Return Enumerable.Empty(Of IDoubleBond)()
        End If
        Return InnerEnumerateDoubleBondInEther(carbon, doubleBond)
    End Function

    Private Function InnerEnumerateDoubleBondInEther(carbon As Integer, doubleBond As IDoubleBond) As IEnumerable(Of IDoubleBond)
        Dim [set] = New HashSet(Of Integer)(doubleBond.Bonds.[Select](Function(bond) bond.Position))
        For i = 0 To doubleBond.UnDecidedCount - 1
            [set].Add(carbon - i * 3)
        Next
        Dim sup = SetToBitArray(carbon, doubleBond)
        Dim result As IEnumerable(Of IDoubleBond) = GenerateDoubleBonds([set], sup, carbon - doubleBond.UnDecidedCount * 3, carbon).ToArray()
        If doubleBond.UnDecidedCount >= 1 Then
            sup.Add(1)
            [set] = New HashSet(Of Integer)(doubleBond.Bonds.[Select](Function(bond) bond.Position))
            [set].Add(1)
            For i = 0 To doubleBond.UnDecidedCount - 1 - 1
                [set].Add(carbon - i * 3)
            Next
            result = result.Concat(GenerateDoubleBonds([set], sup, carbon - (doubleBond.UnDecidedCount - 1) * 3, carbon))
        End If
        Return result
    End Function

    Private Function EnumerateDoubleBondInPlasmalogen(carbon As Integer, doubleBond As IDoubleBond) As IEnumerable(Of IDoubleBond)
        If doubleBond.UnDecidedCount = 0 Then
            Return [Return](doubleBond)
        End If
        If Not DoubleBondIsValid(carbon, doubleBond.Count - 1) Then
            Return Enumerable.Empty(Of IDoubleBond)()
        End If
        Return InnerEnumerateDoubleBondInPlasmalogen(carbon, doubleBond)
    End Function

    Private Function InnerEnumerateDoubleBondInPlasmalogen(carbon As Integer, doubleBond As IDoubleBond) As IEnumerable(Of IDoubleBond)
        Dim [set] = New HashSet(Of Integer)(doubleBond.Bonds.[Select](Function(bond) bond.Position))
        For i = 0 To doubleBond.UnDecidedCount - 1
            [set].Add(carbon - i * 3)
        Next
        Return GenerateDoubleBonds([set], SetToBitArray(carbon, doubleBond), carbon - doubleBond.UnDecidedCount * 3, carbon)
    End Function

    Private Function EnumerateDoubleBondInSphingosine(carbon As Integer, doubleBond As IDoubleBond) As IEnumerable(Of IDoubleBond)
        If doubleBond.UnDecidedCount = 0 Then
            Return {doubleBond}
        End If
        Return GenerateDoubleBondsForSphingosine(doubleBond, carbon)
    End Function

    Private Iterator Function GenerateDoubleBonds([set] As HashSet(Of Integer), sup As HashSet(Of Integer), nextHead As Integer, prevTail As Integer) As IEnumerable(Of IDoubleBond)
        If nextHead = prevTail Then
            If [set].IsSupersetOf(sup) Then
                Yield BitArrayToBond([set])
            End If
            Return
        End If
        While nextHead > 3
            If Not [set].Contains(nextHead) Then
                [set].Add(nextHead)
                [set].Remove(prevTail)
                If [set].IsSupersetOf(sup) Then
                    Yield BitArrayToBond([set])
                End If
                prevTail -= 3
                If sup.Contains(prevTail) Then
                    Exit While
                End If
            End If
            nextHead -= 3
        End While
    End Function

    Private Function GenerateDoubleBondsForSphingosine(doubleBond As IDoubleBond, length As Integer) As IEnumerable(Of IDoubleBond)
        Dim sets = New HashSet(Of Integer)(doubleBond.Bonds.[Select](Function(b) b.Position))

        Dim blank = 3
        If length > 4 AndAlso sets.Contains(4) Then Threading.Interlocked.Decrement(blank)
        If length > 8 AndAlso sets.Contains(8) Then Threading.Interlocked.Decrement(blank)
        If length > 14 AndAlso sets.Contains(14) Then Threading.Interlocked.Decrement(blank)

        Dim bs As IEnumerable(Of IDoubleBond) = {doubleBond}
        Dim [next] = bs
        Dim result = Enumerable.Empty(Of IDoubleBond)()
        If length > 4 AndAlso Not sets.Contains(4) Then
            Dim tmp = [next].[Select](Function(b) b.Decide(DoubleBondInfo.Create(4))).ToArray()
            result = result.Concat(tmp.Where(Function(b) b.UnDecidedCount = 0))
            [next] = [next].Concat(tmp.Where(Function(b) b.UnDecidedCount > 0))
            sets.Add(4)
        End If
        If length > 8 AndAlso Not sets.Contains(8) Then
            Dim tmp = [next].[Select](Function(b) b.Decide(DoubleBondInfo.Create(8))).ToArray()
            result = result.Concat(tmp.Where(Function(b) b.UnDecidedCount = 0))
            [next] = [next].Concat(tmp.Where(Function(b) b.UnDecidedCount > 0))
            sets.Add(8)
        End If
        If length > 14 AndAlso Not sets.Contains(14) Then
            Dim tmp = [next].[Select](Function(b) b.Decide(DoubleBondInfo.Create(14))).ToArray()
            result = result.Concat(tmp.Where(Function(b) b.UnDecidedCount = 0))
            [next] = [next].Concat(tmp.Where(Function(b) b.UnDecidedCount > 0))
            sets.Add(14)
        End If

        If blank <= doubleBond.UnDecidedCount Then
            [next] = [next].Where(Function(b) b.UnDecidedCount > 0 AndAlso {4, 8, 14}.All(Function(i) b.Bonds.[Select](Function(bd) bd.Position).Contains(i)))
            For i = 6 To length - 1 Step 2
                If Not sets.Contains(i) Then
                    sets.Add(i)
                    Dim tmp = [next].[Select](Function(b) b.Decide(DoubleBondInfo.Create(i))).ToArray()
                    result = result.Concat(tmp.Where(Function(b) b.UnDecidedCount = 0))
                    [next] = [next].Concat(tmp.Where(Function(b) b.UnDecidedCount > 0))
                End If
            Next
        End If
        Return result
    End Function

    Private Function SetToBitArray(length As Integer, doubleBond As IDoubleBond) As HashSet(Of Integer)
        If doubleBond.DecidedCount = 0 Then
            Return New HashSet(Of Integer)()
        End If
        Dim result = New HashSet(Of Integer)()
        For Each bond In doubleBond.Bonds
            result.Add(bond.Position)
        Next
        Return result
    End Function

    Private Function BitArrayToBond(arr As HashSet(Of Integer)) As IDoubleBond
        Dim bonds = New List(Of IDoubleBondInfo)()
        For Each v In arr.OrderBy(Function(v) v)
            bonds.Add(DoubleBondInfo.Create(v))
        Next
        Return New DoubleBond(bonds.Count, bonds)
    End Function

    Private Function EnumerateOxidized(carbon As Integer, oxidized As IOxidized, begin As Integer, [end] As Integer) As IEnumerable(Of IOxidized)
        If oxidized.UnDecidedCount = 0 Then
            Return [Return](oxidized)
        End If

        ''' Cannot convert LocalFunctionStatementSyntax, CONVERSION ERROR: Conversion for LocalFunctionStatement not implemented, please report this issue in 'System.Collections.Generic....' at character 12878
        ''' 
        ''' 
        ''' Input:
        ''' 
        System.Collections.Generic.IEnumerable<CompMs.Common.Lipidomics.IOxidized> rec(int i, System.Collections.Generic.List<int> infos) {
                if (infos.Count == oxidized.UnDecidedCount) {
                    yield return CompMs.Common.Lipidomics.Oxidized.CreateFromPosition(oxidized.Oxidises.Concat(infos).OrderBy(p => p).ToArray());
                    yield break;
                }
                for (var j = i; j <carbon + 1; j++){ 
                    if (oxidized.Oxidises.Contains(j)) {
                        continue;
                    }
                    infos.Add(j);
                    ForEach(var res In rec(j + 1, infos)) {
                        yield return res;
                    }
                    infos.RemoveAt(infos.Count - 1);
                }
            }

''' 

            Return rec(begin, New List(Of Integer)(oxidized.UnDecidedCount))
    End Function
End Class