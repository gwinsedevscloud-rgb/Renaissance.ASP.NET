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
import org.ecews.renaissance.domain.Triage;
import org.ecews.renaissance.dtos.TriageDto;
import org.ecews.renaissance.repositories.PatientRepository;
import org.ecews.renaissance.repositories.TriageRepository;
import org.ecews.renaissance.utils.Utils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.BeanUtils;
import org.springframework.data.domain.PageRequest;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class TriageService {
    
    private static final Logger log = LoggerFactory.getLogger(TriageService.class);
    private final TriageRepository triageRepository;
    private final PatientRepository patientRepository;
    private final Utils utils;
    
    public List<TriageDto> getAllTriages() {
        return triageRepository.findAllByArchivedFalse(PageRequest.of(0, 10)).stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }
    
    public TriageDto getTriageById(UUID id) {
        Triage triage = triageRepository.findById(id)
                .filter(t -> !t.isArchived())
                .orElseThrow(() -> new EntityNotFoundException("Triage not found or archived"));
        return convertToDto(triage);
    }
    
    public TriageDto createTriage(TriageDto dto) {
        Triage triage = convertToEntity(dto);
        Patient patient = patientRepository.findById(dto.getPatientId())
                .orElseThrow(() -> new EntityNotFoundException("Patient not found"));
        triage.setPatient(patient);
        triage.setCreatedBy(utils.getCurrentUserName());
        triage.setCreatedDate(LocalDateTime.now());
        triage.setUpdatedBy(utils.getCurrentUserName());
        triage.setUpdatedDate(LocalDateTime.now());
        return convertToDto(triageRepository.save(triage));
    }
    
    public TriageDto updateTriage(UUID id, TriageDto updatedDto) {
        Triage existing = triageRepository.findById(id)
                .filter(t -> !t.isArchived())
                .orElseThrow(() -> new EntityNotFoundException("Triage not found or archived"));
        Patient patient = patientRepository.findById(updatedDto.getPatientId())
                .orElseThrow(() -> new EntityNotFoundException("Patient not found"));
        existing.setPatient(patient);
        
        BeanUtils.copyProperties(updatedDto, existing, "id", "createdBy", "createdDate", "archived", "patient");
        existing.setUpdatedBy(utils.getCurrentUserName());
        existing.setUpdatedDate(LocalDateTime.now());
        
        return convertToDto(triageRepository.save(existing));
    }
    
    @Transactional
    public void archiveTriage(UUID id) {
        Triage existing = triageRepository.findById(id)
                .filter(t -> !t.isArchived())
                .orElseThrow(() -> new EntityNotFoundException("Triage not found or already archived"));
        
        existing.setArchived(true);
        existing.setUpdatedBy(getCurrentAuditor());
        existing.setUpdatedDate(LocalDateTime.now());
        triageRepository.save(existing);
    }
    
    private String getCurrentAuditor() {
        return "system";
    }
    
    private TriageDto convertToDto(Triage triage) {
        TriageDto dto = new TriageDto();
        BeanUtils.copyProperties(triage, dto);
        dto.setPatientId(triage.getPatient().getId());
        return dto;
    }
    
    public List<TriageDto> getTriagesByPatientId(UUID patientId) {
        return triageRepository.findAllByPatient_IdAndArchivedFalse(patientId).stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }
    
    private Triage convertToEntity(TriageDto dto) {
        Triage entity = new Triage();
        BeanUtils.copyProperties(dto, entity);
        return entity;
    }
}
