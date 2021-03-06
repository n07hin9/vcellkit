﻿#Region "Microsoft.VisualBasic::5e47a195bdc3a30b400216b72754b5d9, IO\GCMarkupLanguage\v2\ModelExtensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Module ModelExtensions
    ' 
    '         Function: createFluxes, createGenotype, CreateModel, createPhenotype, exportRegulations
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.ComponentModel.EquaionModel.DefaultTypes
Imports SMRUCC.genomics.GCModeller.ModellingEngine.Model
Imports FluxModel = SMRUCC.genomics.GCModeller.ModellingEngine.Model.Reaction

Namespace v2

    Public Module ModelExtensions

        ''' <summary>
        ''' 将所加载的XML模型文件转换为统一的数据模型
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateModel(model As VirtualCell) As CellularModule
            Dim genotype As New Genotype With {
                .centralDogmas = model _
                    .createGenotype _
                    .OrderByDescending(Function(gene) gene.RNA.Value) _
                    .ToArray,
                .ProteinMatrix = model.genome.replicons _
                    .Select(Function(rep) rep.genes.AsEnumerable) _
                    .IteratesALL _
                    .Where(Function(gene) Not gene.amino_acid Is Nothing) _
                    .Select(Function(gene)
                                Return gene.amino_acid.DoCall(AddressOf ProteinFromVector)
                            End Function) _
                    .ToArray,
                .RNAMatrix = model.genome.replicons _
                    .Select(Function(rep) rep.genes.AsEnumerable) _
                    .IteratesALL _
                    .Select(Function(rna)
                                Return rna.nucleotide_base.DoCall(AddressOf RNAFromVector)
                            End Function) _
                    .ToArray
            }

            Return New CellularModule With {
                .Taxonomy = model.taxonomy,
                .Genotype = genotype,
                .Phenotype = model.createPhenotype,
                .Regulations = model.exportRegulations.ToArray
            }
        End Function

        <Extension>
        Private Iterator Function createGenotype(model As VirtualCell) As IEnumerable(Of CentralDogma)
            Dim genomeName$
            Dim enzymes As Dictionary(Of String, Enzyme) = model.metabolismStructure _
                .enzymes _
                .ToDictionary(Function(enzyme) enzyme.geneID)
            Dim rnaTable As Dictionary(Of String, NamedValue(Of RNATypes))
            Dim RNA As NamedValue(Of RNATypes)
            Dim proteinId$

            For Each replicon In model.genome.replicons
                genomeName = replicon.genomeName
                rnaTable = replicon _
                    .RNAs _
                    .AsEnumerable _
                    .ToDictionary(Function(r) r.gene,
                                  Function(r)
                                      Return New NamedValue(Of RNATypes) With {
                                          .Name = r.gene,
                                          .Value = r.type,
                                          .Description = r.val
                                      }
                                  End Function)

                For Each gene As gene In replicon.genes.AsEnumerable
                    If rnaTable.ContainsKey(gene.locus_tag) Then
                        RNA = rnaTable(gene.locus_tag)
                        proteinId = Nothing
                    Else
                        ' 枚举的默认值为mRNA
                        RNA = New NamedValue(Of RNATypes) With {
                            .Name = gene.locus_tag
                        }
                        proteinId = gene.protein_id Or $"{gene.locus_tag}::peptide".AsDefault
                    End If

                    Yield New CentralDogma With {
                        .replicon = genomeName,
                        .geneID = gene.locus_tag,
                        .polypeptide = proteinId,
                        .orthology = enzymes.TryGetValue(.geneID)?.KO,
                        .RNA = RNA
                    }
                Next
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function createPhenotype(model As VirtualCell) As Phenotype
            Dim fluxChannels = model.createFluxes _
                .OrderByDescending(Function(r) r.enzyme.SafeQuery.Count) _
                .ToArray
            Dim enzymes = model.metabolismStructure.enzymes _
                .Select(Function(enz) enz.geneID) _
                .ToArray
            Dim proteins = model.genome.replicons _
                .Select(Function(genome)
                            Return genome.genes.AsEnumerable
                        End Function) _
                .IteratesALL _
                .Where(Function(gene) Not gene.amino_acid Is Nothing) _
                .Select(Function(orf)
                            Return New Protein With {
                                .compounds = {},
                                .polypeptides = {orf.protein_id},
                                .ProteinID = orf.protein_id
                            }
                        End Function) _
                .ToArray

            Return New Phenotype With {
                .fluxes = fluxChannels,
                .enzymes = enzymes,
                .proteins = proteins
            }
        End Function

        <Extension>
        Private Iterator Function createFluxes(model As VirtualCell) As IEnumerable(Of FluxModel)
            Dim equation As Equation
            ' {reactionID => KO()}
            Dim enzymes = model.metabolismStructure _
                .enzymes _
                .Select(Function(enz)
                            Return enz _
                                .catalysis _
                                .Select(Function(ec)
                                            Return (rID:=ec.reaction, enz:=enz.KO)
                                        End Function)
                        End Function) _
                .IteratesALL _
                .GroupBy(Function(r) r.rID) _
                .ToDictionary(Function(r) r.Key,
                              Function(g)
                                  Return g.Select(Function(r) r.enz) _
                                          .Distinct _
                                          .ToArray
                              End Function)
            Dim KO$()
            Dim bounds As Double()

            For Each reaction As Reaction In model.metabolismStructure.reactions.AsEnumerable
                equation = Equation.TryParse(reaction.Equation)

                If reaction.is_enzymatic Then
                    KO = enzymes.TryGetValue(reaction.ID, mute:=True)

                    If KO.IsNullOrEmpty Then
                        ' 当前的基因组内没有对应的酶来催化这个反应过程
                        ' 则限制一个很小的range
                        bounds = {10, 10}
                    Else
                        bounds = {500, 1000.0}
                    End If
                Else
                    KO = {}
                    bounds = {200, 200.0}
                End If

                If Not equation.reversible Then
                    ' only forward flux direction
                    bounds(Scan0) = 0
                End If

                Yield New FluxModel With {
                    .ID = reaction.ID,
                    .name = reaction.name,
                    .substrates = equation.Reactants _
                        .Select(Function(c) c.AsFactor) _
                        .ToArray,
                    .products = equation.Products _
                        .Select(Function(c) c.AsFactor) _
                        .ToArray,
                    .enzyme = KO,
                    .bounds = bounds
                }
            Next
        End Function

        <Extension>
        Private Iterator Function exportRegulations(model As VirtualCell) As IEnumerable(Of Regulation)
            For Each reg As transcription In model.genome.regulations
                Yield New Regulation With {
                    .effects = reg.mode.EvalEffects,
                    .name = reg.biological_process,
                    .process = reg.centralDogma,
                    .regulator = reg.regulator,
                    .type = Processes.Transcription
                }
            Next
        End Function
    End Module
End Namespace
