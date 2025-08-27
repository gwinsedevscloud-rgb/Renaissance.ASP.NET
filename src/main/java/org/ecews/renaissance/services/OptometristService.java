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
import org.ecews.renaissance.domain.Optometrist;
import org.ecews.renaissance.domain.Patient;
import org.ecews.renaissance.dtos.OptometristDTO;
import org.ecews.renaissance.repositories.OptometristRepository;
import org.ecews.renaissance.repositories.PatientRepository;
import org.ecews.renaissance.utils.Utils;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class OptometristService {
    
    private final OptometristRepository repository;
    private final PatientRepository patientRepository;
    private final Utils utils;
    
    public Optometrist save(OptometristDTO dto) {
        Patient patient = patientRepository.findById(dto.getPatientId())
                .orElseThrow(() -> new IllegalArgumentException("Invalid patient ID"));
        
        Optometrist o = new Optometrist();
        o.setId(dto.getId());
        o.setPatient(patient);
        o.setVisualAcuityLeft(dto.getVisualAcuityLeft());
        o.setVisualAcuityRight(dto.getVisualAcuityRight());
        o.setGlassesDispensed(dto.getGlassesDispensed());
        o.setReferred(dto.getReferred());
        o.setCreatedBy(utils.getCurrentUserName());
        o.setCreatedDate(LocalDateTime.now());
        o.setUpdatedDate(LocalDateTime.now());
        o.setUpdatedBy(utils.getCurrentUserName());
        
        return repository.save(o);
    }
    
    public List<Optometrist> saveAll(List<OptometristDTO> dtos) {
        return dtos.stream().map(this::save).toList();
    }
    
    public Optometrist update(UUID id, OptometristDTO dto) {
        Optometrist o = repository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Optometrist record not found"));
        
        o.setVisualAcuityLeft(dto.getVisualAcuityLeft());
        o.setVisualAcuityRight(dto.getVisualAcuityRight());
        o.setGlassesDispensed(dto.getGlassesDispensed());
        o.setReferred(dto.getReferred());
        o.setUpdatedBy(utils.getCurrentUserName());
        o.setUpdatedDate(LocalDateTime.now());
        
        return repository.save(o);
    }
    
    public void delete(UUID id) {
        Optometrist o = repository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Optometrist record not found"));
        o.setArchived(true);
        repository.save(o);
    }
    
    public List<Optometrist> findByPatientId(UUID patientId) {
        try {
            return repository.findByPatientId(patientId);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
    
    public Optometrist findById(UUID id) {
        return repository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Optometrist record not found"));
    }
    
    public List<Optometrist> findAll() {
        return repository.findAll().stream()
                .filter(p -> !p.isArchived())
                .toList();
    }
}
