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

import jakarta.persistence.EntityNotFoundException;
import lombok.RequiredArgsConstructor;
import org.ecews.renaissance.domain.Patient;
import org.ecews.renaissance.dtos.PatientDto;
import org.ecews.renaissance.repositories.PatientRepository;
import org.ecews.renaissance.utils.Utils;
import org.springframework.beans.BeanUtils;
import org.springframework.data.domain.AuditorAware;
import org.springframework.data.domain.PageRequest;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class PatientService {
    
    private final PatientRepository patientRepository;
    private final Utils utils;
    
    public List<PatientDto> getAllPatients() {
        return patientRepository.findAllByArchivedFalse(PageRequest.of(0, 10)).stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }
    
    public PatientDto getPatientById(UUID id) {
        Patient patient = patientRepository.findById(id)
                .filter(p -> !p.isArchived())
                .orElseThrow(() -> new EntityNotFoundException("Patient not found or archived"));
        return convertToDto(patient);
    }
    
    public PatientDto createPatient(PatientDto patientDto) {
        try {
            Patient patient = convertToEntity(patientDto);
            patient.setCreatedBy(getCurrentAuditor());
            patient.setCreatedDate(LocalDateTime.now());
            patient.setUpdatedDate(LocalDateTime.now());
            patient.setUpdatedBy(getCurrentAuditor());
            return convertToDto(patientRepository.save(patient));
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
    
    public PatientDto updatePatient(UUID id, PatientDto updatedDto) {
        Patient existing = patientRepository.findById(id)
                .filter(p -> !p.isArchived())
                .orElseThrow(() -> new EntityNotFoundException("Patient not found or archived"));
        
        BeanUtils.copyProperties(updatedDto, existing, "id", "createdBy", "createdDate", "archived");
        existing.setUpdatedBy(getCurrentAuditor());
        existing.setUpdatedDate(LocalDateTime.now());
        
        return convertToDto(patientRepository.save(existing));
    }
    
    @Transactional
    public void archivePatient(UUID id) {
        Patient existing = patientRepository.findById(id)
                .filter(p -> !p.isArchived())
                .orElseThrow(() -> new EntityNotFoundException("Patient not found or already archived"));
        
        existing.setArchived(true);
        existing.setUpdatedBy(getCurrentAuditor());
        existing.setUpdatedDate(LocalDateTime.now());
        patientRepository.save(existing);
    }
    
    private String getCurrentAuditor() {
        return utils.getCurrentUserName();
    }
    
    private PatientDto convertToDto(Patient patient) {
        PatientDto dto = new PatientDto();
        BeanUtils.copyProperties(patient, dto);
        return dto;
    }
    
    public List<PatientDto> getPatientByClientNumber(String clientNumber) {
        return patientRepository.findByClientNumberContainingIgnoreCaseAndArchivedFalseOrderByCreatedDateDesc(clientNumber)
                .stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }
    
    private Patient convertToEntity(PatientDto dto) {
        Patient entity = new Patient();
        BeanUtils.copyProperties(dto, entity);
        return entity;
    }
}
