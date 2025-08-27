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
package org.ecews.renaissance.services;

import lombok.RequiredArgsConstructor;
import org.ecews.renaissance.domain.DentalConsultation;
import org.ecews.renaissance.domain.Patient;
import org.ecews.renaissance.dtos.DentalConsultationDto;
import org.ecews.renaissance.repositories.DentalConsultationRepository;
import org.ecews.renaissance.repositories.PatientRepository;
import org.ecews.renaissance.utils.Utils;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class DentalConsultationService {
    
    private final DentalConsultationRepository dentalRepo;
    private final PatientRepository patientRepo;
    private final Utils utils;
    
    public DentalConsultation save(DentalConsultationDto dto) {
        Patient patient = patientRepo.findById(dto.getPatientId())
                .orElseThrow(() -> new IllegalArgumentException("Invalid patient ID"));
        
        DentalConsultation dc = new DentalConsultation();
        dc.setPatient(patient);
        dc.setDiagnoses(dto.getDiagnoses());
        dc.setTreatments(dto.getTreatments());
        dc.setCreatedBy(utils.getCurrentUserName());
        dc.setCreatedDate(LocalDateTime.now());
        dc.setUpdatedBy(utils.getCurrentUserName());
        dc.setUpdatedDate(LocalDateTime.now());
        dc.setReferred(dto.getReferred());
        dc.setServicesReferred(dto.getServicesReferred());
        dc.setOthersDiagnosis(dto.getOthersDiagnosis());
        dc.setOthersTreatment(dto.getOthersTreatment());
        
        return dentalRepo.save(dc);
    }
    
    public List<DentalConsultation> getByPatient(UUID patientId) {
        return dentalRepo.findByPatientId(patientId);
    }
    
    public List<DentalConsultation> getAll() {
        return dentalRepo.findAll();
    }
    
    public DentalConsultation update(UUID id, DentalConsultationDto dto) {
        DentalConsultation dc = dentalRepo.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Dental Consultation not found"));
        
        dc.setDiagnoses(dto.getDiagnoses());
        dc.setTreatments(dto.getTreatments());
        dc.setUpdatedBy(utils.getCurrentUserName());
        dc.setUpdatedDate(LocalDateTime.now());
        dc.setReferred(dto.getReferred());
        dc.setServicesReferred(dto.getServicesReferred());
        dc.setOthersDiagnosis(dto.getOthersDiagnosis());
        dc.setOthersTreatment(dto.getOthersTreatment());
        
        return dentalRepo.save(dc);
    }
    
    public void delete(UUID id) {
        dentalRepo.deleteById(id);
    }
    
    public DentalConsultation findById(UUID id) {
        return dentalRepo.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Dental Consultation not found"));
    }
    
    public List<DentalConsultation> findByPatientId(UUID patientId) {
        return dentalRepo.findByPatientId(patientId);
    }
}
