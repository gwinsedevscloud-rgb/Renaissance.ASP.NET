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
import org.ecews.renaissance.domain.PharmacyPrescription;
import org.ecews.renaissance.domain.enums.DispensationStatus;
import org.ecews.renaissance.dtos.PharmacyPrescriptionDto;
import org.ecews.renaissance.repositories.PatientRepository;
import org.ecews.renaissance.repositories.PharmacyPrescriptionRepository;
import org.ecews.renaissance.utils.Utils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.BeanUtils;
import org.springframework.data.domain.AuditorAware;
import org.springframework.data.domain.PageRequest;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class PharmacyPrescriptionService {
    
    private static final Logger log = LoggerFactory.getLogger(PharmacyPrescriptionService.class);
    private final PharmacyPrescriptionRepository repository;
    private final PatientRepository patientRepository;
    private final Utils utils;
    
    public void saveAll(List<PharmacyPrescriptionDto> prescriptions) {
        log.info("Pharmacy data size is {}", prescriptions.size());
        try {
            List<PharmacyPrescription> entities = prescriptions.stream().map(dto -> {
                Patient patient = patientRepository.findById(dto.getPatientId())
                        .orElseThrow(() -> new IllegalArgumentException("Invalid patient ID"));
                
                PharmacyPrescription pres = new PharmacyPrescription();
                pres.setPatient(patient);
                pres.setDrugCategory(dto.getDrugCategory());
                pres.setDrugName(dto.getDrugName());
                pres.setDosage(dto.getDosage());
                pres.setFrequency(dto.getFrequency());
                pres.setDuration(dto.getDuration());
                pres.setDispensed(dto.isDispensed());
                pres.setDispensationNote(dto.getDispensationNote());
                pres.setCreatedBy(utils.getCurrentUserName());
                pres.setCreatedDate(LocalDateTime.now());
                pres.setUpdatedBy(utils.getCurrentUserName());
                pres.setUpdatedDate(LocalDateTime.now());
                return pres;
            }).toList();
            
            repository.saveAll(entities);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
    
    public void updateAll(List<PharmacyPrescriptionDto> dtos) {
        var prescriptions = dtos.stream().map(dto -> {
            PharmacyPrescription entity = repository.findById(dto.getId())
                    .orElseThrow(() -> new IllegalArgumentException("Prescription not found: " + dto.getId()));
            
            entity.setDispensed(dto.isDispensed());
            entity.setDispensationNote(dto.getDispensationNote());
            entity.setStatus(dto.getStatus());
            entity.setQuantityDispensed(dto.getQuantityDispensed());
            return entity;
        }).toList();
        
        repository.saveAll(prescriptions);
    }
    
    public List<PharmacyPrescriptionDto> getAllPrescriptions() {
        return repository.findAllByArchivedFalse(PageRequest.of(0, 10)).stream()
                .map(this::toDto)
                .collect(Collectors.toList());
    }
    
    public PharmacyPrescriptionDto getById(UUID id) {
        return repository.findById(id)
                .filter(p -> !p.isArchived())
                .map(this::toDto)
                .orElseThrow(() -> new EntityNotFoundException("Prescription not found or archived"));
    }
    
    public PharmacyPrescriptionDto create(PharmacyPrescriptionDto dto) {
        PharmacyPrescription entity = toEntity(dto);
        Patient patient = patientRepository.findById(dto.getPatientId())
                .orElseThrow(() -> new EntityNotFoundException("Patient not found"));
        entity.setPatient(patient);
        entity.setCreatedBy(utils.getCurrentUserName());
        entity.setCreatedDate(LocalDateTime.now());
        entity.setUpdatedBy(utils.getCurrentUserName());
        entity.setUpdatedDate(LocalDateTime.now());
        return toDto(repository.save(entity));
    }
    
    public PharmacyPrescriptionDto update(UUID id, PharmacyPrescriptionDto dto) {
        PharmacyPrescription entity = repository.findById(id)
                .filter(p -> !p.isArchived())
                .orElseThrow(() -> new EntityNotFoundException("Prescription not found or archived"));
        BeanUtils.copyProperties(dto, entity, "id", "patient", "createdBy", "createdDate", "archived");
        entity.setUpdatedBy(utils.getCurrentUserName());
        entity.setUpdatedDate(LocalDateTime.now());
        return toDto(repository.save(entity));
    }
    
    @Transactional
    public void archive(UUID id) {
        PharmacyPrescription entity = repository.findById(id)
                .filter(p -> !p.isArchived())
                .orElseThrow(() -> new EntityNotFoundException("Prescription not found or archived"));
        entity.setArchived(true);
        entity.setUpdatedBy(utils.getCurrentUserName());
        entity.setUpdatedDate(LocalDateTime.now());
        repository.save(entity);
    }
    
    public List<PharmacyPrescriptionDto> getByPatientId(UUID patientId) {
        return repository.findByPatient_IdAndArchivedFalse(patientId).stream()
                .map(this::toDto)
                .collect(Collectors.toList());
    }
    
    private PharmacyPrescriptionDto toDto(PharmacyPrescription entity) {
        PharmacyPrescriptionDto dto = new PharmacyPrescriptionDto();
        BeanUtils.copyProperties(entity, dto);
        dto.setPatientId(entity.getPatient().getId());
        return dto;
    }
    
    private PharmacyPrescription toEntity(PharmacyPrescriptionDto dto) {
        PharmacyPrescription entity = new PharmacyPrescription();
        BeanUtils.copyProperties(dto, entity);
        return entity;
    }
    
    public Boolean hasPendingPrescriptions(UUID patientId) {
        return repository.existsByPatientIdAndStatusAndArchivedFalse(patientId, DispensationStatus.PENDING);
    }
}
