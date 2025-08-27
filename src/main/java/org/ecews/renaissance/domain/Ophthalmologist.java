/*
* MIT License
*
* Copyright (c) 2025 [AUTHOR OR ORGANIZATION]
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
package org.ecews.renaissance.domain;

import jakarta.persistence.*;
import lombok.Data;
import org.hibernate.annotations.GenericGenerator;

import java.util.List;
import java.util.UUID;

@Data
@Entity
@Table(name = "ren_ophthalmologist")
public class Ophthalmologist extends AuditEntity {
    
    @Id
    @GeneratedValue
    @GenericGenerator(name = "uuid2", strategy = "uuid2")
    @Column(columnDefinition = "BINARY(16)")
    private UUID id;
    
    @Column(name = "patient_id", nullable = false)
    private UUID patientId;
    
    @Column(name = "referred")
    private Boolean referred;
    
    @Column(name = "diagnosis")
    private List<String> diagnoses;
    
    @Column(name = "treatments")
    private List<String> treatments;
    
    @Column(name = "surgery")
    private List<String> surgeries;
    
    private List<String> othersDiagnosis;
    private List<String> othersTreatment;
    private List<String> otherSurgery;
    
    private String visualAcuityRight;
    private String visualAcuityLeft;
    private Boolean glassesDispensed;
}
