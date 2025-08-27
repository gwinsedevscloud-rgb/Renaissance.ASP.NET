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
import org.ecews.renaissance.domain.Consultation;
import org.ecews.renaissance.domain.Patient;
import org.ecews.renaissance.dtos.ConsultationDto;
import org.ecews.renaissance.repositories.ConsultationRepository;
import org.ecews.renaissance.repositories.PatientRepository;
import org.ecews.renaissance.utils.Utils;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class ConsultationService {
    
    private final ConsultationRepository consultationRepository;
    private final PatientRepository patientRepository;
    private final Utils utils;
    
    public Consultation save(ConsultationDto dto) {
        Patient patient = patientRepository.findById(dto.getPatientId())
                .orElseThrow(() -> new IllegalArgumentException("Invalid patient ID"));
        
        Consultation c = new Consultation();
        c.setPatient(patient);
        c.setDiagnoses(dto.getDiagnoses());
        c.setTreatments(dto.getTreatments());
        c.setServicesReferred(dto.getServicesReferred());
        c.setReferred(dto.getReferred());
        c.setItnOrder(dto.getItnOrder());
        c.setItnDispense(dto.getItnDispense());
        c.setCreatedBy(utils.getCurrentUserName());
        c.setCreatedDate(LocalDateTime.now());
        c.setUpdatedDate(LocalDateTime.now());
        c.setUpdatedBy(utils.getCurrentUserName());
        c.setOthersDiagnosis(dto.getOthersDiagnosis());
        c.setOthersTreatment(dto.getOthersTreatment());
        
        return consultationRepository.save(c);
    }
    
    public List<Consultation> saveAll(List<ConsultationDto> dtos) {
        return dtos.stream().map(this::save).toList();
    }
    
    public Consultation update(UUID id, ConsultationDto dto) {
        Consultation c = consultationRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Consultation not found"));
        
        c.setDiagnoses(dto.getDiagnoses());
        c.setTreatments(dto.getTreatments());
        c.setServicesReferred(dto.getServicesReferred());
        c.setReferred(dto.getReferred());
        c.setItnOrder(dto.getItnOrder());
        c.setItnDispense(dto.getItnDispense());
        c.setUpdatedBy(utils.getCurrentUserName());
        c.setUpdatedDate(LocalDateTime.now());
        c.setOthersDiagnosis(dto.getOthersDiagnosis());
        c.setOthersTreatment(dto.getOthersTreatment());
        return consultationRepository.save(c);
    }
    
    public void delete(UUID id) {
        Consultation c = consultationRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Consultation not found"));
        c.setArchived(true);
        consultationRepository.save(c);
    }
    
    public List<Consultation> findByPatientId(UUID patientId) {
        return consultationRepository.findByPatientIdAndArchivedFalse(patientId);
    }
    
    public Consultation findById(UUID id) {
        return consultationRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Consultation not found"));
    }
    
    public List<Consultation> findAll() {
        return consultationRepository.findAll().stream()
                .filter(c -> !c.isArchived())
                .toList();
    }
    
    public Consultation updateItnDispense(UUID id, Boolean itnDispense) {
        Consultation consultation = consultationRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Consultation not found"));
        
        consultation.setItnDispense(itnDispense);
        return consultationRepository.save(consultation);
    }
}
