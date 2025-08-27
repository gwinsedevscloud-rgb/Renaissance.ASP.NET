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
import org.ecews.renaissance.domain.Laboratory;
import org.ecews.renaissance.domain.Patient;
import org.ecews.renaissance.dtos.LaboratoryDto;
import org.ecews.renaissance.repositories.LaboratoryRepository;
import org.ecews.renaissance.repositories.PatientRepository;
import org.ecews.renaissance.utils.Utils;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class LaboratoryService {
    
    private final LaboratoryRepository laboratoryRepository;
    private final PatientRepository patientRepository;
    private final Utils utils;
    
    public Laboratory save(LaboratoryDto dto) {
        Patient patient = patientRepository.findById(dto.getPatientId())
                .orElseThrow(() -> new IllegalArgumentException("Invalid patient ID"));
        
        Laboratory lab = new Laboratory();
        lab.setPatient(patient);
        lab.setTestName(dto.getTestName());
        lab.setResult(dto.getResult());
        lab.setNote(dto.getNote());
        lab.setCreatedBy(utils.getCurrentUserName());
        lab.setCreatedDate(LocalDateTime.now());
        lab.setUpdatedBy(utils.getCurrentUserName());
        lab.setUpdatedDate(LocalDateTime.now());
        
        return laboratoryRepository.save(lab);
    }
    
    public List<Laboratory> saveAll(List<LaboratoryDto> dtos) {
        return dtos.stream().map(this::save).toList();
    }
    
    public Laboratory update(UUID id, LaboratoryDto dto) {
        Laboratory lab = laboratoryRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Lab record not found"));
        
        lab.setTestName(dto.getTestName());
        lab.setResult(dto.getResult());
        lab.setNote(dto.getNote());
        lab.setUpdatedBy(utils.getCurrentUserName());
        lab.setUpdatedDate(LocalDateTime.now());
        
        return laboratoryRepository.save(lab);
    }
    
    public void delete(UUID id) {
        Laboratory lab = laboratoryRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Lab record not found"));
        lab.setArchived(true);
        laboratoryRepository.save(lab);
    }
    
    public List<Laboratory> findByPatientId(UUID patientId) {
        return laboratoryRepository.findByPatientIdAndArchivedFalse(patientId);
    }
    
    public Laboratory findById(UUID id) {
        return laboratoryRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Lab record not found"));
    }
    
    public List<Laboratory> findAll() {
        return laboratoryRepository.findAll().stream()
                .filter(lab -> !lab.isArchived())
                .toList();
    }
}
