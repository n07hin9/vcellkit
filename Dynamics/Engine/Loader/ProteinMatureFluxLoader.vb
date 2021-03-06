﻿#Region "Microsoft.VisualBasic::7797cd7e9c2d11b7b1cfcb5d0668f8b3, Dynamics\Engine\Loader\ProteinMatureFluxLoader.vb"

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

    '     Class ProteinMatureFluxLoader
    ' 
    '         Properties: polypeptides, proteinComplex
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateFlux
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.GCModeller.ModellingEngine.Dynamics.Core
Imports SMRUCC.genomics.GCModeller.ModellingEngine.Model

Namespace Engine.ModelLoader

    ''' <summary>
    ''' 构建酶成熟的过程
    ''' </summary>
    Public Class ProteinMatureFluxLoader : Inherits FluxLoader

        Public ReadOnly Property polypeptides As String()
        Public ReadOnly Property proteinComplex As String()

        Public Sub New(loader As Loader)
            MyBase.New(loader)
        End Sub

        Public Overrides Iterator Function CreateFlux(cell As CellularModule) As IEnumerable(Of Channel)
            Dim polypeptides As New List(Of String)
            Dim proteinComplex As New List(Of String)

            For Each complex As Protein In cell.Phenotype.proteins
                For Each compound In complex.compounds
                    If Not MassTable.Exists(compound) Then
                        Call MassTable.AddNew(compound)
                    End If
                Next
                For Each peptide In complex.polypeptides
                    If Not MassTable.Exists(peptide) Then
                        Throw New MissingMemberException(peptide)
                    Else
                        polypeptides += peptide
                    End If
                Next

                Dim unformed = MassTable.variables(complex).ToArray
                Dim complexID As String = MassTable.AddNew(complex.ProteinID & ".complex")
                Dim mature As Variable = MassTable.variable(complexID)

                proteinComplex += complexID

                ' 酶的成熟过程也是一个不可逆的过程
                Yield New Channel(unformed, {mature}) With {
                    .ID = complex.DoCall(AddressOf Loader.GetProteinMatureId),
                    .reverse = New Controls With {.baseline = 0},
                    .forward = New Controls With {.baseline = loader.dynamics.proteinMatureBaseline},
                    .bounds = New Boundary With {
                        .forward = loader.dynamics.proteinMatureCapacity,
                        .reverse = 0
                    }
                }
            Next

            _polypeptides = polypeptides
            _proteinComplex = proteinComplex
        End Function
    End Class
End Namespace
